import { inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { ConfirmationService } from 'primeng/api';
import { Observable } from 'rxjs';

export interface HasPendingChanges {
  hasPendingChanges(): boolean;
}

/**
 * Prevents users from accidentally navigating away from unsaved forms.
 * Prompts with a standardized confirmation modal using PrimeNG ConfirmationService.
 */
export const pendingChangesGuard: CanDeactivateFn<HasPendingChanges> = (component) => {
  if (!component || !component.hasPendingChanges || !component.hasPendingChanges()) {
    return true;
  }

  const confirmationService = inject(ConfirmationService);

  return new Observable<boolean>((observer) => {
    confirmationService.confirm({
      header: 'Unsaved Changes',
      message: 'You have unsaved changes. Are you sure you want to discard them and leave this page?',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Discard & Leave',
      rejectLabel: 'Stay Here',
      acceptButtonStyleClass: 'p-button-danger p-button-sm',
      rejectButtonStyleClass: 'p-button-secondary p-button-sm',
      accept: () => {
        observer.next(true);
        observer.complete();
      },
      reject: () => {
        observer.next(false);
        observer.complete();
      }
    });
  });
};
