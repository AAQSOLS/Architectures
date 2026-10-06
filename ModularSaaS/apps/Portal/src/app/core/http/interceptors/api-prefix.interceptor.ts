import { HttpInterceptorFn } from '@angular/common/http';

export const apiPrefixInterceptor: HttpInterceptorFn = (req, next) => {
  // Allow absolute external URLs (e.g. CDNs or third-party webhooks) to pass unmodified
  if (req.url.startsWith('http://') || req.url.startsWith('https://') || req.url.startsWith('/i18n/')) {
    return next(req);
  }

  // In production or development with reverse proxy, relative API paths are routed directly
  return next(req);
};
