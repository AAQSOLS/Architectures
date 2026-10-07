/**
 * Injects X-Tenant-Id header for backend multi-tenant resolution.
 */
export function tenantInterceptor(
  headers: Headers,
  tenantId?: string | null
): void {
  if (tenantId && !headers.has("X-Tenant-Id")) {
    headers.set("X-Tenant-Id", tenantId);
  }
}
