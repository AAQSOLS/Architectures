import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-forbidden',
  standalone: true,
  imports: [RouterModule, ButtonModule],
  template: `
    <div class="flex-col items-center justify-center gap-4 surface-ground" style="min-height: 80vh; text-align: center;">
      <i class="pi pi-lock" style="font-size: 3rem; color: var(--color-warning);"></i>
      <h1 class="text-2xl font-bold text-primary">Access Restricted (403)</h1>
      <p class="text-sm text-secondary" style="max-width: 400px;">
        Your account does not possess the required permission keys to access this resource.
      </p>
      <p-button
        label="Return to Dashboard"
        icon="pi pi-arrow-left"
        routerLink="/dashboard"
        severity="primary"
        size="small"
      />
    </div>
  `
})
export class ForbiddenComponent {}
