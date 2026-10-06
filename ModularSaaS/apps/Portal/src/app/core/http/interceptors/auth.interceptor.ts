import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import { AuthStore } from '../../auth/auth.store';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const store = inject(AuthStore);
  const authService = inject(AuthService);
  const token = store.token();

  const isAuthEndpoint = req.url.includes('/auth/login') || req.url.includes('/auth/refresh');

  let authReq = req;
  if (token && !isAuthEndpoint && !req.headers.has('Authorization')) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Catch 401 Unauthorized on non-auth endpoints
      if (error.status === 401 && !isAuthEndpoint) {
        if (!isRefreshing) {
          isRefreshing = true;
          refreshTokenSubject.next(null);

          return authService.refreshToken().pipe(
            switchMap(response => {
              isRefreshing = false;
              refreshTokenSubject.next(response.accessToken);
              return next(authReq.clone({
                setHeaders: {
                  Authorization: `Bearer ${response.accessToken}`
                }
              }));
            }),
            catchError(refreshError => {
              isRefreshing = false;
              authService.logout();
              return throwError(() => refreshError);
            })
          );
        } else {
          // Concurrency lock: queue parallel requests until token refresh finishes
          return refreshTokenSubject.pipe(
            filter(newToken => newToken !== null),
            take(1),
            switchMap(newToken => next(authReq.clone({
              setHeaders: {
                Authorization: `Bearer ${newToken}`
              }
            })))
          );
        }
      }

      return throwError(() => error);
    })
  );
};
