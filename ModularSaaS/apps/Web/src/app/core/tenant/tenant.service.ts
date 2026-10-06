import { Injectable, computed, signal } from '@angular/core';

export interface TenantInfo {
  id: string;
  name: string;
  slug: string;
  primaryColor?: string;
  logoUrl?: string;
}

const STORAGE_KEY_TENANT_ID = 'modular_saas_tenant_id';

@Injectable({ providedIn: 'root' })
export class TenantService {
  private readonly _currentTenant = signal<TenantInfo | null>(null);

  readonly currentTenant = this._currentTenant.asReadonly();
  readonly tenantId = computed(() => this._currentTenant()?.id ?? null);
  readonly hasTenant = computed(() => this.tenantId() !== null);

  constructor() {
    this.restoreTenantFromStorage();
  }

  setTenant(tenant: TenantInfo | null): void {
    this._currentTenant.set(tenant);

    if (tenant?.id) {
      localStorage.setItem(STORAGE_KEY_TENANT_ID, tenant.id);
      if (tenant.primaryColor) {
        document.documentElement.style.setProperty('--color-primary-600', tenant.primaryColor);
      }
    } else {
      localStorage.removeItem(STORAGE_KEY_TENANT_ID);
      document.documentElement.style.removeProperty('--color-primary-600');
    }
  }

  private restoreTenantFromStorage(): void {
    const savedId = localStorage.getItem(STORAGE_KEY_TENANT_ID);
    if (savedId) {
      // Default placeholder restored from storage until full profile loads
      this._currentTenant.set({
        id: savedId,
        name: 'Active Workspace',
        slug: 'workspace'
      });
    }
  }
}
