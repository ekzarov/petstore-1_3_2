import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SupplierApiService } from './supplier-api.service';
import { InventoryItem } from '../admin/admin.models';
import { LoadingStateComponent } from '../shared/loading-state.component';
import { UnavailableStateComponent } from '../shared/unavailable-state.component';

@Component({
  selector: 'app-supplier-inventory',
  standalone: true,
  imports: [FormsModule, LoadingStateComponent, UnavailableStateComponent],
  template: `
    <h1 class="cart-title">Supplier inventory</h1>
    <p class="identity-form__hint">
      Saving a quantity automatically ships waiting approved orders for that item.
    </p>

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

      <div class="admin-bulk supplier-recovery">
        <button type="button" (click)="runFulfillment()" [disabled]="busy()">Re-run fulfillment (recovery)</button>
        @if (runResult() !== null) {
          <span class="identity-form__hint">Processed {{ runResult() }} eligible orders.</span>
        }
        <span class="identity-form__hint">
          Normally not needed: saving a quantity already triggers shipping.
        </span>
      </div>
    }
  `
})
export class InventoryComponent implements OnInit {
  private readonly api = inject(SupplierApiService);

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
        // Reload: auto-fulfillment may have decremented the saved value already.
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
