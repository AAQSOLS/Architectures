import { Injectable, computed, signal } from '@angular/core';

export interface AuthUser {
  id: string;
  email: string;
  fullName: string;
  tenantId?: string;
  roles: string[];
}

const STORAGE_KEY_TOKEN = 'modular_saas_access_token';
const STORAGE_KEY_REFRESH = 'modular_saas_refresh_token';

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly _user = signal<AuthUser | null>(null);
  private readonly _permissions = signal<ReadonlySet<string>>(new Set());
  private readonly _token = signal<string | null>(localStorage.getItem(STORAGE_KEY_TOKEN));

  readonly user = this._user.asReadonly();
  readonly token = this._token.asReadonly();
  readonly permissions = this._permissions.asReadonly();

  readonly isAuthenticated = computed(() => this._token() !== null);
  readonly userFullName = computed(() => this._user()?.fullName ?? 'Anonymous User');
  readonly userEmail = computed(() => this._user()?.email ?? '');

  setSession(token: string, refreshToken?: string, user?: AuthUser, permissions: string[] = []): void {
    this._token.set(token);
    localStorage.setItem(STORAGE_KEY_TOKEN, token);

    if (refreshToken) {
      localStorage.setItem(STORAGE_KEY_REFRESH, refreshToken);
    }

    if (user) {
      this._user.set(user);
    }

    this._permissions.set(new Set(permissions));
  }

  setAccessToken(newToken: string): void {
    this._token.set(newToken);
    localStorage.setItem(STORAGE_KEY_TOKEN, newToken);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(STORAGE_KEY_REFRESH);
  }

  hasPermission(key: string): boolean {
    return this._permissions().has(key);
  }

  hasAnyPermission(keys: string[]): boolean {
    const current = this._permissions();
    return keys.some(k => current.has(k));
  }

  clear(): void {
    this._token.set(null);
    this._user.set(null);
    this._permissions.set(new Set());
    localStorage.removeItem(STORAGE_KEY_TOKEN);
    localStorage.removeItem(STORAGE_KEY_REFRESH);
  }
}
