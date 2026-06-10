import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-unavailable-state',
  standalone: true,
  template: `
    <div class="state-unavailable">
      <p>{{ message }}</p>
    </div>
  `
})
export class UnavailableStateComponent {
  @Input() message = 'The catalog is currently unavailable. Please try again later.';
}
