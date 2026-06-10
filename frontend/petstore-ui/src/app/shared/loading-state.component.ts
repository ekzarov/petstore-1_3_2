import { Component } from '@angular/core';

@Component({
  selector: 'app-loading-state',
  standalone: true,
  template: `
    <div class="state-loading">
      <span class="state-loading__spinner" aria-hidden="true"></span>
      <p>Loading…</p>
    </div>
  `
})
export class LoadingStateComponent {}
