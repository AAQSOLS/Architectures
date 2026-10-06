import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { retry, timer } from 'rxjs';

/**
 * Retries idempotent requests (GET, HEAD) on transient network or gateway dropouts (0, 503, 504).
 * Never retries state-changing writes (POST, PUT, DELETE).
 */
export const retryInterceptor: HttpInterceptorFn = (req, next) => {
  const isIdempotent = ['GET', 'HEAD', 'OPTIONS'].includes(req.method.toUpperCase());

  if (!isIdempotent) {
    return next(req);
  }

  return next(req).pipe(
    retry({
      count: 2,
      delay: (error: HttpErrorResponse, retryCount: number) => {
        // Retry only on transient gateway failures or network disconnects
        const isTransient = error.status === 0 || error.status === 503 || error.status === 504;
        if (!isTransient) {
          throw error;
        }
        // Exponential backoff: 500ms, then 1000ms
        return timer(retryCount * 500);
      }
    })
  );
};
