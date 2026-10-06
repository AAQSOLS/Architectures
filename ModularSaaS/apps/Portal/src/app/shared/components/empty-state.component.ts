import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, ButtonModule],
  template: `
    <div class="surface-card flex-col items-center justify-center gap-3" style="text-align: center; padding: var(--space-10) var(--space-6);">
      <div class="flex-row items-center justify-center" style="width: 56px; height: 56px; border-radius: var(--radius-full); background-color: var(--color-surface-hover); color: var(--color-text-secondary); font-size: 1.5rem;">
        <i [class]="icon"></i>
      </div>

      <div class="flex-col gap-1" style="max-width: 360px;">
        <h3 class="text-base font-bold text-primary">{{ title }}</h3>
        <p class="text-xs text-secondary">{{ description }}</p>
      </div>

      @if (actionLabel) {
        <p-button
          [label]="actionLabel"
          [icon]="actionIcon"
          size="small"
          severity="primary"
          (onClick)="actionClicked.emit()"
        />
      }
    </div>
  `
})
export class EmptyStateComponent {
  @Input() icon = 'pi pi-inbox';
  @Input() title = 'No data available';
  @Input() description = 'There are no records matching your current filter criteria.';
  @Input() actionLabel?: string;
  @Input() actionIcon = 'pi pi-plus';
  @Output() actionClicked = new EventEmitter<void>();
}
