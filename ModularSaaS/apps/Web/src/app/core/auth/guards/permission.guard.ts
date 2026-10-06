import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '../auth.store';

export const permissionGuard = (requiredPermission: string): CanActivateFn => {
  return () => {
    const store = inject(AuthStore);
    const router = inject(Router);

    if (store.hasPermission(requiredPermission)) {
      return true;
    }

    return router.createUrlTree(['/forbidden']);
  };
};
