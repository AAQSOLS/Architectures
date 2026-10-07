const RETRYABLE_STATUS_CODES = new Set([503, 504]);
const IDEMPOTENT_METHODS = new Set(["GET", "HEAD", "OPTIONS"]);

export interface RetryConfig {
  maxRetries?: number;
  baseDelayMs?: number;
  maxDelayMs?: number;
}

const DEFAULT_CONFIG: Required<RetryConfig> = {
  maxRetries: 3,
  baseDelayMs: 300,
  maxDelayMs: 3000,
};

function calculateDelay(
  attempt: number,
  baseMs: number,
  maxMs: number
): number {
  const exponential = baseMs * Math.pow(2, attempt);
  const randomArray = new Uint32Array(1);
  crypto.getRandomValues(randomArray);
  const jitterFraction = (randomArray[0] ?? 0) / 0xffffffff;
  const jitter = jitterFraction * baseMs;
  return Math.min(exponential + jitter, maxMs);
}

/**
 * Retries transient fetch errors using exponential backoff with jitter.
 */
export async function executeWithRetry<T>(
  action: () => Promise<T>,
  method: string,
  config?: RetryConfig
): Promise<T> {
  const isIdempotent = IDEMPOTENT_METHODS.has(method.toUpperCase());
  if (!isIdempotent) {
    return action();
  }

  const { maxRetries, baseDelayMs, maxDelayMs } = {
    ...DEFAULT_CONFIG,
    ...config,
  };

  let lastError: unknown;

  for (let attempt = 0; attempt <= maxRetries; attempt++) {
    try {
      return await action();
    } catch (err: unknown) {
      lastError = err;

      const isNetworkError = err instanceof TypeError;
      const status =
        typeof err === "object" && err !== null && "status" in err
          ? (err as { status: number }).status
          : undefined;

      const isRetryable =
        isNetworkError || (status && RETRYABLE_STATUS_CODES.has(status));

      if (!isRetryable || attempt === maxRetries) {
        throw err;
      }

      const delay = calculateDelay(attempt, baseDelayMs, maxDelayMs);
      await new Promise((resolve) => setTimeout(resolve, delay));
    }
  }

  throw lastError;
}
