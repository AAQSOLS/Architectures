import { Injectable, inject } from '@angular/core';
import { TenantService } from '../tenant/tenant.service';

@Injectable({ providedIn: 'root' })
export class StorageService {
  private readonly tenantService = inject(TenantService);

  private buildKey(key: string, isTenantScoped: boolean): string {
    const tenantId = this.tenantService.tenantId();
    if (isTenantScoped && tenantId) {
      return `t_${tenantId}_${key}`;
    }
    return `global_${key}`;
  }

  getItem<T>(key: string, isTenantScoped = true): T | null {
    try {
      const fullKey = this.buildKey(key, isTenantScoped);
      const raw = localStorage.getItem(fullKey);
      if (raw === null) return null;
      return JSON.parse(raw) as T;
    } catch {
      return null;
    }
  }

  setItem<T>(key: string, value: T, isTenantScoped = true): boolean {
    try {
      const fullKey = this.buildKey(key, isTenantScoped);
      localStorage.setItem(fullKey, JSON.stringify(value));
      return true;
    } catch {
      // Gracefully handle storage quota exceeded or private browsing restrictions
      return false;
    }
  }

  removeItem(key: string, isTenantScoped = true): void {
    try {
      const fullKey = this.buildKey(key, isTenantScoped);
      localStorage.removeItem(fullKey);
    } catch {
      // Ignore storage errors on removal
    }
  }

  /**
   * Purges all keys belonging to the current tenant.
   * Useful during tenant switching or team account logout.
   */
  clearCurrentTenantData(): void {
    const tenantId = this.tenantService.tenantId();
    if (!tenantId) return;

    try {
      const prefix = `t_${tenantId}_`;
      const keysToRemove: string[] = [];

      for (let i = 0; i < localStorage.length; i++) {
        const k = localStorage.key(i);
        if (k?.startsWith(prefix)) {
          keysToRemove.push(k);
        }
      }

      for (const k of keysToRemove) {
        localStorage.removeItem(k);
      }
    } catch {
      // Ignore storage errors on purge
    }
  }
}
