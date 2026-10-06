import { HttpInterceptorFn } from '@angular/common/http';

export const HEADER_IDEMPOTENCY_KEY = 'Idempotency-Key';

/**
 * Attaches a unique Idempotency-Key on mutating HTTP operations (POST, PUT, PATCH).
 * Protects backend commands from duplicate execution on network retry or double-click.
 */
export const idempotencyInterceptor: HttpInterceptorFn = (req, next) => {
  const isMutatingMethod = ['POST', 'PUT', 'PATCH'].includes(req.method.toUpperCase());
  const isAuthOrLogin = req.url.includes('/auth/login') || req.url.includes('/auth/refresh');

  if (isMutatingMethod && !isAuthOrLogin && !req.headers.has(HEADER_IDEMPOTENCY_KEY)) {
    const key = crypto.randomUUID();
    const cloned = req.clone({
      setHeaders: {
        [HEADER_IDEMPOTENCY_KEY]: key
      }
    });
    return next(cloned);
  }

  return next(req);
};
