/**
 * Async, timing, debounce, and throttle utilities.
 */

/**
 * Pauses execution for the specified milliseconds.
 * Example: await sleep(1000);
 */
export function sleep(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * Creates a debounced function that delays invoking the callback until after delayMs milliseconds.
 */
export function debounce<T extends (...args: unknown[]) => void>(
  fn: T,
  delayMs = 300
): (...args: Parameters<T>) => void {
  let timerId: ReturnType<typeof setTimeout> | undefined;

  return (...args: Parameters<T>): void => {
    if (timerId !== undefined) {
      clearTimeout(timerId);
    }
    timerId = setTimeout(() => {
      fn(...args);
    }, delayMs);
  };
}

/**
 * Creates a throttled function that only invokes the callback at most once per limitMs milliseconds.
 */
export function throttle<T extends (...args: unknown[]) => void>(
  fn: T,
  limitMs = 300
): (...args: Parameters<T>) => void {
  let inThrottle = false;

  return (...args: Parameters<T>): void => {
    if (!inThrottle) {
      fn(...args);
      inThrottle = true;
      setTimeout(() => {
        inThrottle = false;
      }, limitMs);
    }
  };
}

/**
 * Retries an asynchronous promise function up to a maximum number of attempts with backoff.
 */
export async function retryAsync<T>(
  fn: () => Promise<T>,
  retries = 2,
  delayMs = 500
): Promise<T> {
  try {
    return await fn();
  } catch (error) {
    if (retries <= 0) {
      throw error;
    }
    await sleep(delayMs);
    return retryAsync(fn, retries - 1, delayMs * 1.5);
  }
}
