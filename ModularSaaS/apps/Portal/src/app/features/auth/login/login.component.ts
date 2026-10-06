import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../../core/auth/auth.service';
import { TenantService } from '../../../core/tenant/tenant.service';
import { ToastService } from '../../../core/feedback/toast.service';
import { AppNormalizedError } from '../../../core/http/models/problem-details.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonModule, CardModule, InputTextModule, PasswordModule, MessageModule],
  template: `
    <div class="flex-row items-center justify-center surface-ground" style="min-height: 100vh; padding: var(--space-4);">
      <div class="surface-card flex-col gap-6" style="width: 100%; max-width: 420px; box-shadow: var(--shadow-lg);">
        <!-- Logo & Header -->
        <div class="flex-col items-center justify-center gap-2" style="text-align: center;">
          <div class="flex-row items-center justify-center" style="width: 48px; height: 48px; border-radius: var(--radius-lg); background-color: var(--color-primary); color: var(--color-primary-text); font-weight: bold; font-size: 1.5rem; box-shadow: var(--shadow-md);">
            S
          </div>
          <h1 class="text-xl font-bold text-primary">
            Modular SaaS Portal
          </h1>
          <p class="text-xs text-secondary">
            Sign in to access your tenant dashboard
          </p>
        </div>

        <!-- Form -->
        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="flex-col gap-4">
          <div class="flex-col gap-1">
            <label for="email" class="text-xs font-semibold text-secondary" style="text-transform: uppercase;">
              Email Address
            </label>
            <input
              pInputText
              id="email"
              type="email"
              formControlName="email"
              placeholder="user@example.com"
              class="w-full"
            />
            @if (fieldErrors()['email']) {
              <small class="text-danger">{{ fieldErrors()['email'] }}</small>
            }
          </div>

          <div class="flex-col gap-1">
            <label for="password" class="text-xs font-semibold text-secondary" style="text-transform: uppercase;">
              Password
            </label>
            <p-password
              id="password"
              formControlName="password"
              [feedback]="false"
              [toggleMask]="true"
              styleClass="w-full"
              inputStyleClass="w-full"
              placeholder="••••••••"
            />
            @if (fieldErrors()['password']) {
              <small class="text-danger">{{ fieldErrors()['password'] }}</small>
            }
          </div>

          <!-- Optional Tenant Identifier -->
          <div class="flex-col gap-1">
            <label for="tenantId" class="text-xs font-semibold text-secondary" style="text-transform: uppercase;">
              Tenant ID (Optional)
            </label>
            <input
              pInputText
              id="tenantId"
              type="text"
              formControlName="tenantId"
              placeholder="GUID (leave empty for platform admin)"
              class="w-full"
            />
          </div>

          <p-button
            type="submit"
            label="Sign In"
            icon="pi pi-sign-in"
            [loading]="isLoading()"
            [disabled]="loginForm.invalid"
            styleClass="w-full"
            severity="primary"
          />
        </form>
      </div>
    </div>
  `
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly tenant = inject(TenantService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  readonly isLoading = signal(false);
  readonly fieldErrors = signal<Record<string, string>>({});

  readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    tenantId: ['']
  });

  onSubmit(): void {
    if (this.loginForm.invalid) return;

    this.isLoading.set(true);
    this.fieldErrors.set({});

    const val = this.loginForm.getRawValue();

    if (val.tenantId) {
      this.tenant.setTenant({
        id: val.tenantId,
        name: 'Specified Tenant',
        slug: 'tenant'
      });
    }

    this.auth.login({
      email: val.email,
      password: val.password,
      tenantId: val.tenantId || undefined
    }).subscribe({
      next: () => {
        this.isLoading.set(false);
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err: AppNormalizedError) => {
        this.isLoading.set(false);
        if (err.fieldErrors) {
          this.fieldErrors.set(err.fieldErrors);
        }
      }
    });
  }
}
