import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../../feedback/toast.service';
import { AppNormalizedError, ProblemDetails } from '../models/problem-details.model';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // 401 errors are handled by authInterceptor
      if (error.status === 401) {
        return throwError(() => error);
      }

      const problem: ProblemDetails | null =
        error.error && typeof error.error === 'object' && 'title' in error.error
          ? (error.error as ProblemDetails)
          : null;

      const normalized: AppNormalizedError = {
        status: error.status,
        title: problem?.title ?? 'Error',
        message: problem?.detail ?? error.message ?? 'An unexpected error occurred.',
        fieldErrors: {},
        isValidationError: false,
        raw: problem ?? error.error
      };

      // Extract field validation errors if present (supports both PropertyName and Code shapes)
      if (problem?.errors && Array.isArray(problem.errors)) {
        normalized.isValidationError = true;
        for (const err of problem.errors) {
          const field = err.propertyName ?? err.code ?? 'general';
          const msg = err.errorMessage ?? err.description ?? 'Invalid value';
          normalized.fieldErrors[field] = msg;
        }
      }

      switch (error.status) {
        case 400:
          if (normalized.isValidationError) {
            toast.warning(normalized.message || 'Please correct the validation errors in the form.');
          } else {
            toast.error(normalized.message, normalized.title);
          }
          break;

        case 403:
          toast.error('You do not have permission to access this resource.', 'Access Denied');
          break;

        case 404:
          toast.info(normalized.message || 'The requested resource was not found.', 'Not Found');
          break;

        case 422:
          // Business Rule Violation (from DomainException)
          toast.warning(normalized.message, normalized.title || 'Business Rule Violation');
          break;

        case 429:
          toast.warning('Too many requests. Please wait before trying again.', 'Rate Limited');
          break;

        case 500:
        default:
          toast.error(
            'An unexpected error occurred on the server. Please contact support.',
            'Server Error'
          );
          break;
      }

      return throwError(() => normalized);
    })
  );
};
