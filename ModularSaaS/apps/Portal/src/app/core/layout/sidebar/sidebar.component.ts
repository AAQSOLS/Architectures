import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthStore } from '../../auth/auth.store';
import { LanguageService } from '../../i18n/language.service';

interface NavItem {
  labelKey: string;
  defaultLabel: string;
  route: string;
  permission?: string;
  iconClass: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <aside class="layout-sidebar">
      <!-- App Branding -->
      <div class="flex-row items-center gap-3" style="height: var(--header-height); padding-inline: var(--space-6); border-bottom: 1px solid var(--color-surface-border);">
        <div class="flex-row items-center justify-center" style="width: 36px; height: 36px; border-radius: var(--radius-md); background-color: var(--color-primary); color: var(--color-primary-text); font-weight: var(--font-weight-bold); font-size: 1rem; box-shadow: var(--shadow-sm);">
          S
        </div>
        <div class="flex-col">
          <span class="text-sm font-bold text-primary">
            {{ lang.translate('app.title', 'Modular SaaS') }}
          </span>
          <span class="text-xs text-muted font-medium" style="letter-spacing: 0.05em; text-transform: uppercase;">
            Enterprise Portal
          </span>
        </div>
      </div>

      <!-- Navigation Links -->
      <nav class="flex-col gap-1 flex-1" style="overflow-y: auto; padding: var(--space-4);">
        @for (item of visibleNavItems(); track item.route) {
          <a
            [routerLink]="item.route"
            routerLinkActive="active-nav-link"
            class="nav-link flex-row items-center gap-3"
          >
            <i [class]="item.iconClass" style="font-size: 1.1rem;"></i>
            <span>{{ lang.translate(item.labelKey, item.defaultLabel) }}</span>
          </a>
        }
      </nav>

      <!-- Footer Info -->
      <div style="border-top: 1px solid var(--color-surface-border); padding: var(--space-4);" class="text-xs text-muted">
        <span>v1.0.0 &bull; Multi-Tenant Ready</span>
      </div>
    </aside>
  `,
  styles: [`
    .nav-link {
      padding: var(--space-3) var(--space-4);
      border-radius: var(--radius-md);
      color: var(--color-text-secondary);
      text-decoration: none;
      font-weight: var(--font-weight-medium);
      font-size: var(--font-size-sm);
      transition: background-color var(--transition-fast), color var(--transition-fast);
    }
    .nav-link:hover {
      background-color: var(--color-surface-hover);
      color: var(--color-text-primary);
    }
    .active-nav-link {
      background-color: var(--color-primary-subtle);
      color: var(--color-primary);
      font-weight: var(--font-weight-semibold);
    }
  `]
})
export class SidebarComponent {
  readonly authStore = inject(AuthStore);
  readonly lang = inject(LanguageService);

  private readonly allNavItems: NavItem[] = [
    {
      labelKey: 'nav.dashboard',
      defaultLabel: 'Dashboard',
      route: '/dashboard',
      iconClass: 'pi pi-home'
    },
    {
      labelKey: 'nav.properties',
      defaultLabel: 'Properties',
      route: '/properties',
      permission: 'properties.view',
      iconClass: 'pi pi-building'
    },
    {
      labelKey: 'nav.leads',
      defaultLabel: 'Leads & Inquiries',
      route: '/leads',
      permission: 'leads.view',
      iconClass: 'pi pi-users'
    },
    {
      labelKey: 'nav.users',
      defaultLabel: 'User Management',
      route: '/users',
      permission: 'users.manage',
      iconClass: 'pi pi-user-edit'
    },
    {
      labelKey: 'nav.settings',
      defaultLabel: 'Settings',
      route: '/settings',
      iconClass: 'pi pi-cog'
    }
  ];

  readonly visibleNavItems = computed(() => {
    return this.allNavItems.filter(item => {
      if (!item.permission) return true;
      return this.authStore.hasPermission(item.permission);
    });
  });
}
