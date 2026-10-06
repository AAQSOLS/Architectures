import { Injectable, computed, signal } from '@angular/core';

export type SupportedLanguage = 'en' | 'ur';

const STORAGE_KEY_LANG = 'modular_saas_lang';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly rtlLanguages = new Set<string>(['ur', 'ar', 'fa']);
  private readonly _currentLang = signal<SupportedLanguage>('en');

  readonly currentLang = this._currentLang.asReadonly();
  readonly isRtl = computed(() => this.rtlLanguages.has(this._currentLang()));

  // In-memory key-value dictionary for instant lookup without blocking HTTP
  private readonly translations = signal<Record<string, string>>({});

  constructor() {
    this.init();
  }

  private init(): void {
    const saved = localStorage.getItem(STORAGE_KEY_LANG) as SupportedLanguage | null;
    const initialLang: SupportedLanguage = saved === 'ur' ? 'ur' : 'en';
    this.setLanguage(initialLang);
  }

  setLanguage(lang: SupportedLanguage): void {
    this._currentLang.set(lang);
    localStorage.setItem(STORAGE_KEY_LANG, lang);

    const dir = this.rtlLanguages.has(lang) ? 'rtl' : 'ltr';
    document.documentElement.setAttribute('dir', dir);
    document.documentElement.setAttribute('lang', lang);

    this.loadTranslations(lang);
  }

  toggleLanguage(): void {
    this.setLanguage(this._currentLang() === 'en' ? 'ur' : 'en');
  }

  translate(key: string, fallback?: string): string {
    return this.translations()[key] ?? fallback ?? key;
  }

  private async loadTranslations(lang: SupportedLanguage): Promise<void> {
    try {
      const response = await fetch(`/i18n/${lang}.json`);
      if (response.ok) {
        const data = await response.json();
        this.translations.set(data);
      }
    } catch {
      // Graceful fallback to raw keys if translation file unavailable
    }
  }
}
