/**
 * Injects X-Correlation-Id header for Serilog / OpenTelemetry distributed tracing.
 */
export function correlationInterceptor(headers: Headers): void {
  if (!headers.has("X-Correlation-Id")) {
    headers.set("X-Correlation-Id", crypto.randomUUID());
  }
}
