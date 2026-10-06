import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { AvatarModule } from 'primeng/avatar';
import { TooltipModule } from 'primeng/tooltip';
import { AuthService } from '../../auth/auth.service';
import { AuthStore } from '../../auth/auth.store';
import { LanguageService } from '../../i18n/language.service';
import { TenantService } from '../../tenant/tenant.service';
import { ThemeService } from '../../theme/theme.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, ButtonModule, TagModule, AvatarModule, TooltipModule],
  template: `
    <header class="layout-header">
      <!-- Tenant Scope Info -->
      <div class="flex-row items-center gap-3">
        @if (tenantService.hasTenant()) {
          <p-tag
            severity="success"
            [value]="tenantService.currentTenant()?.name || 'Active Tenant'"
            icon="pi pi-building"
          />
        } @else {
          <p-tag
            severity="warn"
            value="Platform Scope"
            icon="pi pi-shield"
          />
        }
      </div>

      <!-- Action Utilities -->
      <div class="flex-row items-center gap-3">
        <!-- Dark Mode Toggle Button -->
        <p-button
          [icon]="themeService.isDark() ? 'pi pi-sun' : 'pi pi-moon'"
          [text]="true"
          severity="secondary"
          size="small"
          (onClick)="themeService.toggleTheme()"
          [pTooltip]="themeService.isDark() ? 'Switch to Light Mode' : 'Switch to Dark Mode'"
        />

        <!-- Language Switcher (EN / UR) -->
        <p-button
          [label]="languageService.currentLang() === 'en' ? 'اردو (Urdu)' : 'English'"
          icon="pi pi-globe"
          [outlined]="true"
          size="small"
          (onClick)="languageService.toggleLanguage()"
        />

        <!-- User Info & Logout -->
        <div class="flex-row items-center gap-2" style="border-inline-start: 1px solid var(--color-surface-border); padding-inline-start: 1rem;">
          <p-avatar
            [label]="authStore.userFullName().charAt(0).toUpperCase()"
            shape="circle"
            [style]="{ 'background-color': 'var(--color-primary)', 'color': 'var(--color-primary-text)' }"
          />
          <div class="flex-col" style="text-align: start;">
            <span class="font-semibold text-xs text-primary">{{ authStore.userFullName() }}</span>
            <span class="text-xs text-secondary">{{ authStore.userEmail() }}</span>
          </div>

          <p-button
            icon="pi pi-sign-out"
            [text]="true"
            severity="danger"
            size="small"
            (onClick)="authService.logout()"
            pTooltip="Sign Out"
          />
        </div>
      </div>
    </header>
  `
})
export class HeaderComponent {
  readonly authStore = inject(AuthStore);
  readonly authService = inject(AuthService);
  readonly tenantService = inject(TenantService);
  readonly languageService = inject(LanguageService);
  readonly themeService = inject(ThemeService);
}
