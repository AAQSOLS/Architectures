import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TenantService } from '../../tenant/tenant.service';

export const tenantInterceptor: HttpInterceptorFn = (req, next) => {
  const tenantService = inject(TenantService);
  const tenantId = tenantService.tenantId();

  // If there's an active tenant and header is not already set, attach X-Tenant-Id
  if (tenantId && !req.headers.has('X-Tenant-Id')) {
    const modifiedReq = req.clone({
      setHeaders: {
        'X-Tenant-Id': tenantId
      }
    });
    return next(modifiedReq);
  }

  return next(req);
};
