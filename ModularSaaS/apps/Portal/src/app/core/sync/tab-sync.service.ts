import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthStore } from '../auth/auth.store';
import { TenantService } from '../tenant/tenant.service';

export type TabSyncMessage =
  | { type: 'LOGOUT' }
  | { type: 'LOGIN'; token: string }
  | { type: 'TENANT_CHANGED'; tenantId: string };

const CHANNEL_NAME = 'modular_saas_sync_channel';

@Injectable({ providedIn: 'root' })
export class TabSyncService {
  private readonly router = inject(Router);
  private readonly authStore = inject(AuthStore);
  private readonly tenantService = inject(TenantService);
  private channel: BroadcastChannel | null = null;

  constructor() {
    this.init();
  }

  private init(): void {
    if (typeof window !== 'undefined' && 'BroadcastChannel' in window) {
      this.channel = new BroadcastChannel(CHANNEL_NAME);
      this.channel.onmessage = (event: MessageEvent<TabSyncMessage>) => {
        this.handleMessage(event.data);
      };
    }
  }

  notifyLogout(): void {
    this.postMessage({ type: 'LOGOUT' });
  }

  notifyTenantChanged(tenantId: string): void {
    this.postMessage({ type: 'TENANT_CHANGED', tenantId });
  }

  private postMessage(msg: TabSyncMessage): void {
    try {
      this.channel?.postMessage(msg);
    } catch {
      // Gracefully handle browser channel exceptions
    }
  }

  private handleMessage(msg: TabSyncMessage): void {
    switch (msg.type) {
      case 'LOGOUT':
        this.authStore.clear();
        this.router.navigate(['/auth/login']);
        break;

      case 'TENANT_CHANGED':
        // Update local tenant state to mirror changed tenant
        this.tenantService.setTenant({
          id: msg.tenantId,
          name: 'Switched Tenant',
          slug: 'tenant'
        });
        break;

      default:
        break;
    }
  }
}
