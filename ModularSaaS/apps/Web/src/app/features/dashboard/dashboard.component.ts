import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { AuthStore } from '../../core/auth/auth.store';
import { LanguageService } from '../../core/i18n/language.service';
import { TenantService } from '../../core/tenant/tenant.service';
import { ThemeService } from '../../core/theme/theme.service';
import { ToastService } from '../../core/feedback/toast.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ButtonModule, CardModule, TagModule],
  template: `
    <div class="flex-col gap-6">
      <!-- Welcome Header -->
      <div class="flex-col gap-1">
        <h1 class="text-2xl font-bold text-primary">
          {{ lang.translate('nav.dashboard', 'Dashboard') }}
        </h1>
        <p class="text-sm text-secondary">
          Welcome back, {{ authStore.userFullName() }}. Here is your tenant workspace overview.
        </p>
      </div>

      <!-- Quick Metrics Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div class="surface-card flex-col gap-2">
          <span class="text-xs font-semibold text-secondary uppercase">Tenant Scope</span>
          <div class="text-xl font-bold text-primary">
            {{ tenantService.currentTenant()?.name ?? 'Platform Scope' }}
          </div>
          <p-tag severity="success" value="Header: X-Tenant-Id" />
        </div>

        <div class="surface-card flex-col gap-2">
          <span class="text-xs font-semibold text-secondary uppercase">Active Language</span>
          <div class="text-xl font-bold text-primary uppercase">
            {{ lang.currentLang() }} ({{ lang.isRtl() ? 'RTL' : 'LTR' }})
          </div>
          <p-tag severity="info" value="Dynamic document direction" />
        </div>

        <div class="surface-card flex-col gap-2">
          <span class="text-xs font-semibold text-secondary uppercase">Active Theme</span>
          <div class="text-xl font-bold text-primary">
            {{ themeService.isDark() ? 'Dark Mode' : 'Light Mode' }}
          </div>
          <p-tag [severity]="themeService.isDark() ? 'contrast' : 'warn'" [value]="themeService.mode() + ' setting'" />
        </div>

        <div class="surface-card flex-col gap-2">
          <span class="text-xs font-semibold text-secondary uppercase">API Error Handler</span>
          <div class="text-xl font-bold text-primary">
            RFC 7807 Ready
          </div>
          <p-tag severity="success" value="ProblemDetails Interceptor" />
        </div>
      </div>

      <!-- Diagnostics Panel -->
      <div class="surface-card flex-col gap-4">
        <div class="flex-col gap-1">
          <h2 class="text-lg font-bold text-primary">
            Backend Contract & Error Interceptor Diagnostics
          </h2>
          <p class="text-sm text-secondary">
            Verify how the frontend intercepts real backend RFC 7807 ProblemDetails and routes them into PrimeNG toasts.
          </p>
        </div>

        <div class="flex-row flex-wrap gap-3">
          <p-button
            label="Trigger Success Toast"
            icon="pi pi-check"
            severity="success"
            size="small"
            (onClick)="testSimulateSuccess()"
          />

          <p-button
            label="Simulate 400 Validation Error"
            icon="pi pi-exclamation-triangle"
            severity="warn"
            size="small"
            (onClick)="testSimulateValidationError()"
          />

          <p-button
            label="Simulate 422 Domain Rule Error"
            icon="pi pi-times-circle"
            severity="danger"
            size="small"
            (onClick)="testSimulateDomainError()"
          />

          <!-- Theme custom brand tester -->
          <p-button
            label="Test Tenant Emerald Theme"
            icon="pi pi-palette"
            [outlined]="true"
            size="small"
            (onClick)="themeService.setTenantBrandColor('#059669')"
          />

          <p-button
            label="Reset Default Theme"
            icon="pi pi-refresh"
            [text]="true"
            size="small"
            (onClick)="themeService.setTenantBrandColor(undefined)"
          />
        </div>
      </div>
    </div>
  `
})
export class DashboardComponent {
  readonly authStore = inject(AuthStore);
  readonly lang = inject(LanguageService);
  readonly tenantService = inject(TenantService);
  readonly themeService = inject(ThemeService);
  readonly toast = inject(ToastService);

  testSimulateSuccess(): void {
    this.toast.success('Action completed successfully.', 'Operation Successful');
  }

  testSimulateValidationError(): void {
    this.toast.warning('One or more fields failed validation: Email is already registered.', 'Validation Failed');
  }

  testSimulateDomainError(): void {
    this.toast.error('The tenant cannot be suspended while active contracts exist.', 'Business Rule Violation');
  }
}
