import { Directive, Input, TemplateRef, ViewContainerRef, inject } from '@angular/core';
import { AuthStore } from '../../core/auth/auth.store';

@Directive({
  selector: '[hasPermission]',
  standalone: true
})
export class HasPermissionDirective {
  private readonly store = inject(AuthStore);
  private readonly templateRef = inject(TemplateRef<unknown>);
  private readonly viewContainer = inject(ViewContainerRef);
  private hasView = false;

  @Input() set hasPermission(permissionKey: string) {
    const isAuthorized = this.store.hasPermission(permissionKey);

    if (isAuthorized && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!isAuthorized && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}
