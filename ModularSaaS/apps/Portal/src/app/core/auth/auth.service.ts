import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { AuthStore, AuthUser } from './auth.store';
import { TabSyncService } from '../sync/tab-sync.service';

export interface LoginRequest {
  email: string;
  password: string;
  tenantId?: string | undefined;
}

export interface AuthTokensResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn?: number | undefined;
  user?: AuthUser | undefined;
  permissions?: string[] | undefined;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly store = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly tabSync = inject(TabSyncService);

  login(credentials: LoginRequest): Observable<AuthTokensResponse> {
    return this.http.post<AuthTokensResponse>('/api/v1/auth/login', credentials).pipe(
      tap(response => {
        this.store.setSession(
          response.accessToken,
          response.refreshToken,
          response.user,
          response.permissions ?? []
        );
      })
    );
  }

  refreshToken(): Observable<AuthTokensResponse> {
    const refreshToken = this.store.getRefreshToken();
    return this.http.post<AuthTokensResponse>('/api/v1/auth/refresh', { refreshToken }).pipe(
      tap(response => {
        this.store.setAccessToken(response.accessToken);
      })
    );
  }

  logout(): void {
    this.store.clear();
    this.tabSync.notifyLogout();
    this.router.navigate(['/auth/login']);
  }
}
