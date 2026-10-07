/**
 * Safe, tenant-isolated local storage service preventing cross-tenant data collisions.
 */
class TenantStorageService {
  private getPrefix(tenantId?: string | null): string {
    const tid = tenantId ?? "default";
    return `saas:${tid}:`;
  }

  getItem<T>(key: string, tenantId?: string | null): T | null {
    if (typeof window === "undefined") return null;

    try {
      const fullKey = `${this.getPrefix(tenantId)}${key}`;
      const raw = window.localStorage.getItem(fullKey);
      if (!raw) return null;
      return JSON.parse(raw) as T;
    } catch {
      return null;
    }
  }

  setItem<T>(key: string, value: T, tenantId?: string | null): boolean {
    if (typeof window === "undefined") return false;

    try {
      const fullKey = `${this.getPrefix(tenantId)}${key}`;
      window.localStorage.setItem(fullKey, JSON.stringify(value));
      return true;
    } catch {
      return false;
    }
  }

  removeItem(key: string, tenantId?: string | null): void {
    if (typeof window === "undefined") return;

    try {
      const fullKey = `${this.getPrefix(tenantId)}${key}`;
      window.localStorage.removeItem(fullKey);
    } catch {
      // Ignore storage errors in restricted contexts
    }
  }

  clearTenant(tenantId?: string | null): void {
    if (typeof window === "undefined") return;

    try {
      const prefix = this.getPrefix(tenantId);
      const keysToRemove: string[] = [];

      for (let i = 0; i < window.localStorage.length; i++) {
        const key = window.localStorage.key(i);
        if (key && key.startsWith(prefix)) {
          keysToRemove.push(key);
        }
      }

      keysToRemove.forEach((k) => window.localStorage.removeItem(k));
    } catch {
      // Ignore storage errors
    }
  }
}

export const storageService = new TenantStorageService();
