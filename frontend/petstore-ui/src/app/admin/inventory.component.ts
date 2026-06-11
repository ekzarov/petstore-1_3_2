import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from './admin-api.service';
import { InventoryItem } from './admin.models';
import { LoadingStateComponent } from '../shared/loading-state.component';
import { UnavailableStateComponent } from '../shared/unavailable-state.component';

@Component({
  selector: 'app-admin-inventory',
  standalone: true,
  imports: [FormsModule, LoadingStateComponent, UnavailableStateComponent],
  template: `
    <h1 class="cart-title">Supplier inventory</h1>

    <div class="admin-bulk">
      <button type="button" (click)="runFulfillment()" [disabled]="busy()">Run fulfillment</button>
      @if (runResult() !== null) {
        <span class="identity-form__hint">Processed {{ runResult() }} eligible orders.</span>
      }
    </div>

    @if (error()) {
      <p class="cart-error">{{ error() }}</p>
    }

    @if (state() === 'loading') {
      <app-loading-state />
    } @else if (state() === 'unavailable') {
      <app-unavailable-state />
    } @else if (items().length === 0) {
      <div class="state-empty"><p>No inventory rows.</p></div>
    } @else {
      <table class="cart-table">
        <thead>
          <tr><th>Item</th><th>On hand</th><th>New quantity</th><th></th></tr>
        </thead>
        <tbody>
          @for (item of items(); track item.itemId) {
            <tr>
              <td><span class="cart-table__id">{{ item.itemId }}</span></td>
              <td>{{ item.quantityOnHand }}</td>
              <td>
                <input
                  class="admin-inventory__input"
                  type="number"
                  min="0"
                  [ngModel]="edits()[item.itemId] ?? item.quantityOnHand"
                  (ngModelChange)="setEdit(item.itemId, $event)"
                />
              </td>
              <td>
                <button type="button" class="admin-decide" (click)="save(item.itemId)" [disabled]="busy()">Save</button>
              </td>
            </tr>
          }
        </tbody>
      </table>
    }
  `
})
export class InventoryComponent implements OnInit {
  private readonly api = inject(AdminApiService);

  readonly state = signal<'loading' | 'ready' | 'unavailable'>('loading');
  readonly items = signal<InventoryItem[]>([]);
  readonly edits = signal<Record<string, number>>({});
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);
  readonly runResult = signal<number | null>(null);

  ngOnInit(): void {
    this.load();
  }

  setEdit(itemId: string, value: number): void {
    this.edits.update((edits) => ({ ...edits, [itemId]: value }));
  }

  save(itemId: string): void {
    const quantity = this.edits()[itemId];
    if (quantity === undefined || this.busy()) {
      return;
    }

    if (quantity < 0 || !Number.isInteger(Number(quantity))) {
      this.error.set('Quantity must be a non-negative whole number.');
      return;
    }

    this.busy.set(true);
    this.error.set(null);
    this.api.setInventory(itemId, Number(quantity)).subscribe({
      next: () => {
        this.busy.set(false);
        this.load();
      },
      error: () => {
        this.busy.set(false);
        this.error.set('Saving inventory failed. Please try again.');
      }
    });
  }

  runFulfillment(): void {
    if (this.busy()) {
      return;
    }

    this.busy.set(true);
    this.runResult.set(null);
    this.api.runFulfillment().subscribe({
      next: (result) => {
        this.busy.set(false);
        this.runResult.set(result.processed);
        this.load();
      },
      error: () => {
        this.busy.set(false);
        this.error.set('Fulfillment run failed. Please try again.');
      }
    });
  }

  private load(): void {
    this.api.getInventory().subscribe({
      next: (items) => {
        this.items.set(items);
        this.edits.set({});
        this.state.set('ready');
      },
      error: () => this.state.set('unavailable')
    });
  }
}
