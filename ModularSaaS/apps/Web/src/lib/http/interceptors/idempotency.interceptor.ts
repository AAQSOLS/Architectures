const MUTATING_METHODS = new Set(["POST", "PUT", "PATCH"]);

/**
 * Attaches an Idempotency-Key UUID header to mutating HTTP requests.
 */
export function idempotencyInterceptor(method: string, headers: Headers): void {
  const upperMethod = method.toUpperCase();
  if (MUTATING_METHODS.has(upperMethod) && !headers.has("Idempotency-Key")) {
    headers.set("Idempotency-Key", crypto.randomUUID());
  }
}
