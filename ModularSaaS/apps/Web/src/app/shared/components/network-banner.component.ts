import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { NetworkStatusService } from '../../core/network/network-status.service';

@Component({
  selector: 'app-network-banner',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (!network.isOnline()) {
      <div
        role="status"
        aria-live="assertive"
        class="flex-row items-center justify-center gap-2"
        style="background-color: var(--color-danger); color: #ffffff; padding: var(--space-2) var(--space-4); font-size: var(--font-size-xs); font-weight: var(--font-weight-semibold); width: 100%; position: sticky; top: 0; z-index: 50; box-shadow: var(--shadow-sm);"
      >
        <i class="pi pi-wifi text-base"></i>
        <span>No internet connection. Actions and changes may not be saved.</span>
      </div>
    }
  `
})
export class NetworkBannerComponent {
  readonly network = inject(NetworkStatusService);
}
