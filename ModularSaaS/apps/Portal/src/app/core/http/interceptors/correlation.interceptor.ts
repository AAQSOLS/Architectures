import { HttpInterceptorFn } from '@angular/common/http';

export const HEADER_CORRELATION_ID = 'X-Correlation-Id';

/**
 * Attaches a unique correlation ID to every outgoing request.
 * Aligns client-side errors directly with backend Serilog and OpenTelemetry logs.
 */
export const correlationInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.headers.has(HEADER_CORRELATION_ID)) {
    return next(req);
  }

  const correlationId = crypto.randomUUID();
  const modifiedReq = req.clone({
    setHeaders: {
      [HEADER_CORRELATION_ID]: correlationId
    }
  });

  return next(modifiedReq);
};
