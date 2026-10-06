import { Injectable, computed, effect, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark' | 'system';

const STORAGE_KEY_THEME_MODE = 'modular_saas_theme_mode';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly _mode = signal<ThemeMode>('system');
  private readonly _systemPrefersDark = signal<boolean>(false);

  readonly mode = this._mode.asReadonly();

  readonly isDark = computed(() => {
    const currentMode = this._mode();
    if (currentMode === 'dark') return true;
    if (currentMode === 'light') return false;
    return this._systemPrefersDark();
  });

  constructor() {
    this.init();

    // Effect to apply document attributes whenever mode changes
    effect(() => {
      const dark = this.isDark();
      const root = document.documentElement;

      if (dark) {
        root.setAttribute('data-theme', 'dark');
        root.classList.add('app-dark');
      } else {
        root.setAttribute('data-theme', 'light');
        root.classList.remove('app-dark');
      }
    });
  }

  private init(): void {
    // 1. Detect system preference
    if (typeof window !== 'undefined' && window.matchMedia) {
      const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
      this._systemPrefersDark.set(mediaQuery.matches);
      mediaQuery.addEventListener('change', e => this._systemPrefersDark.set(e.matches));
    }

    // 2. Restore user preference
    const saved = localStorage.getItem(STORAGE_KEY_THEME_MODE) as ThemeMode | null;
    if (saved && (saved === 'light' || saved === 'dark' || saved === 'system')) {
      this._mode.set(saved);
    }
  }

  setThemeMode(mode: ThemeMode): void {
    this._mode.set(mode);
    localStorage.setItem(STORAGE_KEY_THEME_MODE, mode);
  }

  toggleTheme(): void {
    const nextMode: ThemeMode = this.isDark() ? 'light' : 'dark';
    this.setThemeMode(nextMode);
  }

  /**
   * Dynamically applies a tenant's primary brand color to the design system tokens at runtime.
   * @param primaryHex e.g. '#0284c7' or '#10b981'
   */
  setTenantBrandColor(primaryHex?: string): void {
    const root = document.documentElement;
    if (primaryHex) {
      root.style.setProperty('--tenant-color-primary', primaryHex);
    } else {
      root.style.removeProperty('--tenant-color-primary');
    }
  }
}
