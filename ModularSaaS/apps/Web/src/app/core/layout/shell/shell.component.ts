import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastContainerComponent } from '../../../shared/components/toast-container.component';
import { NetworkBannerComponent } from '../../../shared/components/network-banner.component';
import { HeaderComponent } from '../header/header.component';
import { SidebarComponent } from '../sidebar/sidebar.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ConfirmDialogModule,
    NetworkBannerComponent,
    HeaderComponent,
    SidebarComponent,
    ToastContainerComponent
  ],
  template: `
    <!-- Top Offline Banner (Appears only when network is disconnected) -->
    <app-network-banner></app-network-banner>

    <div class="layout-wrapper">
      <!-- Sidebar Navigation -->
      <app-sidebar></app-sidebar>

      <!-- Main Shell Area -->
      <div class="layout-main-container">
        <!-- Header -->
        <app-header></app-header>

        <!-- Main Content Area -->
        <main class="layout-content">
          <router-outlet></router-outlet>
        </main>
      </div>

      <!-- PrimeNG Global Confirmation Dialog -->
      <p-confirmDialog [style]="{ width: '450px' }" />

      <!-- PrimeNG Accessible Toast Container -->
      <app-toast-container></app-toast-container>
    </div>
  `
})
export class ShellComponent {}
