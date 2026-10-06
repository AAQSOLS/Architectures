import { Injectable, inject } from '@angular/core';
import { MessageService } from 'primeng/api';

export type ToastSeverity = 'success' | 'info' | 'warn' | 'error';

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly messageService = inject(MessageService);

  show(severity: ToastSeverity, summary: string, detail: string, life = 5000): void {
    this.messageService.add({
      severity,
      summary,
      detail,
      life
    });
  }

  success(detail: string, summary = 'Success'): void {
    this.show('success', summary, detail);
  }

  error(detail: string, summary = 'Error'): void {
    this.show('error', summary, detail, 7000);
  }

  warning(detail: string, summary = 'Warning'): void {
    this.show('warn', summary, detail, 6000);
  }

  info(detail: string, summary = 'Information'): void {
    this.show('info', summary, detail);
  }

  clear(): void {
    this.messageService.clear();
  }
}
