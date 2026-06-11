import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminApiService } from './admin-api.service';
import { AdminOrderSummary } from './admin.models';
import { LoadingStateComponent } from '../shared/loading-state.component';
import { UnavailableStateComponent } from '../shared/unavailable-state.component';

type Outcome = 'approved' | 'denied' | 'failed';

@Component({
  selector: 'app-pending-orders',
  standalone: true,
  imports: [RouterLink, LoadingStateComponent, UnavailableStateComponent],
  template: `
    <h1 class="cart-title">Pending orders</h1>

    @if (state() === 'loading') {
      <app-loading-state />
    } @else if (state() === 'unavailable') {
      <app-unavailable-state />
    } @else if (orders().length === 0) {
      <div class="state-empty"><p>No orders are waiting for a decision.</p></div>
    } @else {
      <div class="admin-bulk">
        <button type="button" (click)="decideSelected(true)" [disabled]="busy() || selection().size === 0">Approve selected</button>
        <button type="button" class="admin-bulk__deny" (click)="decideSelected(false)" [disabled]="busy() || selection().size === 0">Deny selected</button>
        <span class="identity-form__hint">{{ selection().size }} selected</span>
      </div>

      <table class="cart-table">
        <thead>
          <tr><th></th><th>Order</th><th>Placed</th><th>Customer</th><th>Total</th><th>Actions</th><th></th></tr>
        </thead>
        <tbody>
          @for (order of orders(); track order.orderId) {
            <tr>
              <td>
                <input type="checkbox" [checked]="selection().has(order.orderId)" (change)="toggle(order.orderId)" />
              </td>
              <td><a [routerLink]="['/admin', 'orders', order.orderId]">#{{ order.orderId }}</a></td>
              <td>{{ order.placedAt.slice(0, 10) }}</td>
              <td>{{ order.userId }}</td>
              <td>{{ order.total }} {{ order.currency }}</td>
              <td>
                <button type="button" class="admin-decide" (click)="decide(order.orderId, true)" [disabled]="busy()">Approve</button>
                <button type="button" class="admin-decide admin-decide--deny" (click)="decide(order.orderId, false)" [disabled]="busy()">Deny</button>
              </td>
              <td>
                @if (outcomes()[order.orderId]; as outcome) {
                  <span class="admin-outcome admin-outcome--{{ outcome }}">{{ outcomeLabel(outcome) }}</span>
                }
              </td>
            </tr>
          }
        </tbody>
      </table>
    }
  `
})
export class PendingOrdersComponent implements OnInit {
  private readonly api = inject(AdminApiService);

  readonly state = signal<'loading' | 'ready' | 'unavailable'>('loading');
  readonly orders = signal<AdminOrderSummary[]>([]);
  readonly selection = signal(new Set<string>());
  readonly outcomes = signal<Record<string, Outcome>>({});
  readonly busy = signal(false);

  ngOnInit(): void {
    this.load();
  }

  toggle(orderId: string): void {
    const next = new Set(this.selection());
    if (!next.delete(orderId)) {
      next.add(orderId);
    }
    this.selection.set(next);
  }

  decide(orderId: string, approve: boolean): void {
    this.decideMany([orderId], approve);
  }

  decideSelected(approve: boolean): void {
    this.decideMany([...this.selection()], approve);
  }

  outcomeLabel(outcome: Outcome): string {
    return outcome === 'approved' ? 'Approved' : outcome === 'denied' ? 'Denied' : 'Already decided';
  }

  private decideMany(orderIds: string[], approve: boolean): void {
    if (this.busy() || orderIds.length === 0) {
      return;
    }

    this.busy.set(true);
    let remaining = orderIds.length;
    // Sequential per-order calls with per-order outcome feedback (plan DD-003).
    const next = (index: number) => {
      if (index >= orderIds.length) {
        return;
      }

      const orderId = orderIds[index];
      const call = approve ? this.api.approve(orderId) : this.api.deny(orderId);
      call.subscribe({
        next: () => this.record(orderId, approve ? 'approved' : 'denied', --remaining, index, next),
        error: () => this.record(orderId, 'failed', --remaining, index, next)
      });
    };
    next(0);
  }

  private record(orderId: string, outcome: Outcome, remaining: number, index: number, next: (i: number) => void): void {
    this.outcomes.update((outcomes) => ({ ...outcomes, [orderId]: outcome }));
    if (remaining === 0) {
      this.busy.set(false);
      // Refresh after the batch so decided orders leave the queue, but keep
      // outcome badges visible by re-merging rows that are still pending.
      this.load();
      this.selection.set(new Set());
    } else {
      next(index + 1);
    }
  }

  private load(): void {
    this.api.getOrders('PENDING').subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.state.set('ready');
      },
      error: () => this.state.set('unavailable')
    });
  }
}
