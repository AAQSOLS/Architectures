import { ErrorHandler, Injectable, Injector } from '@angular/core';
import { AuthStore } from '../auth/auth.store';
import { TenantService } from '../tenant/tenant.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  constructor(private readonly injector: Injector) {}

  handleError(error: unknown): void {
    const timestamp = new Date().toISOString();

    // Lazy resolve stores to prevent circular DI during application bootstrapping
    let tenantId: string | null = null;
    let userEmail: string | null = null;

    try {
      const tenantService = this.injector.get(TenantService);
      tenantId = tenantService.tenantId();
    } catch {
      // Ignore injection error during early init
    }

    try {
      const authStore = this.injector.get(AuthStore);
      userEmail = authStore.userEmail();
    } catch {
      // Ignore injection error during early init
    }

    const errorDetails = {
      timestamp,
      tenantId,
      user: userEmail,
      url: typeof window !== 'undefined' ? window.location.href : '',
      message: error instanceof Error ? error.message : String(error),
      stack: error instanceof Error ? error.stack : undefined
    };

    // In production, forward to Sentry / Application Insights telemetry sink
    if (typeof console !== 'undefined' && console.error) {
      console.error('[GlobalErrorHandler caught exception]:', errorDetails);
    }
  }
}
