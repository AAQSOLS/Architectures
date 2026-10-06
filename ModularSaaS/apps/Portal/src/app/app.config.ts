import { ApplicationConfig, ErrorHandler, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { ConfirmationService, MessageService } from 'primeng/api';

import { routes } from './app.routes';
import { GlobalErrorHandler } from './core/error/global-error-handler';
import { apiPrefixInterceptor } from './core/http/interceptors/api-prefix.interceptor';
import { correlationInterceptor } from './core/http/interceptors/correlation.interceptor';
import { tenantInterceptor } from './core/http/interceptors/tenant.interceptor';
import { idempotencyInterceptor } from './core/http/interceptors/idempotency.interceptor';
import { authInterceptor } from './core/http/interceptors/auth.interceptor';
import { retryInterceptor } from './core/http/interceptors/retry.interceptor';
import { errorInterceptor } from './core/http/interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes, withComponentInputBinding()),
    provideAnimationsAsync(),
    provideHttpClient(
      withInterceptors([
        apiPrefixInterceptor,
        correlationInterceptor,
        tenantInterceptor,
        idempotencyInterceptor,
        authInterceptor,
        retryInterceptor,
        errorInterceptor
      ])
    ),
    MessageService,
    ConfirmationService,
    {
      provide: ErrorHandler,
      useClass: GlobalErrorHandler
    }
  ]
};
