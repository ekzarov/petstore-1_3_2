import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="state-empty">
      <p>{{ message }}</p>
    </div>
  `
})
export class EmptyStateComponent {
  @Input() message = 'Nothing here yet.';
}
