import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminApiService } from './admin-api.service';
import { AdminOrderSummary, ORDER_STATUSES } from './admin.models';
import { LoadingStateComponent } from '../shared/loading-state.component';
import { UnavailableStateComponent } from '../shared/unavailable-state.component';

@Component({
  selector: 'app-admin-order-list',
  standalone: true,
  imports: [RouterLink, LoadingStateComponent, UnavailableStateComponent],
  template: `
    <h1 class="cart-title">All orders</h1>

    <div class="admin-filter">
      <button
        type="button"
        class="admin-filter__tab"
        [class.admin-filter__tab--active]="filter() === null"
        (click)="setFilter(null)"
      >All</button>
      @for (status of statuses; track status) {
        <button
          type="button"
          class="admin-filter__tab"
          [class.admin-filter__tab--active]="filter() === status"
          (click)="setFilter(status)"
        >{{ status }}</button>
      }
    </div>

    @if (state() === 'loading') {
      <app-loading-state />
    } @else if (state() === 'unavailable') {
      <app-unavailable-state />
    } @else if (orders().length === 0) {
      <div class="state-empty"><p>No orders match this filter.</p></div>
    } @else {
      <table class="cart-table">
        <thead>
          <tr><th>Order</th><th>Placed</th><th>Customer</th><th>Total</th><th>Status</th></tr>
        </thead>
        <tbody>
          @for (order of orders(); track order.orderId) {
            <tr>
              <td><a [routerLink]="['/admin', 'orders', order.orderId]">#{{ order.orderId }}</a></td>
              <td>{{ order.placedAt.slice(0, 10) }}</td>
              <td>{{ order.userId }}</td>
              <td>{{ order.total }} {{ order.currency }}</td>
              <td><span class="order-status" [class]="'order-status order-status--' + order.status.toLowerCase().replace('_', '-')">{{ order.status }}</span></td>
            </tr>
          }
        </tbody>
      </table>
    }
  `
})
export class AdminOrderListComponent implements OnInit {
  private readonly api = inject(AdminApiService);

  readonly statuses = ORDER_STATUSES;
  readonly filter = signal<string | null>(null);
  readonly state = signal<'loading' | 'ready' | 'unavailable'>('loading');
  readonly orders = signal<AdminOrderSummary[]>([]);

  ngOnInit(): void {
    this.load();
  }

  setFilter(status: string | null): void {
    this.filter.set(status);
    this.load();
  }

  private load(): void {
    this.state.set('loading');
    this.api.getOrders(this.filter() ?? undefined).subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.state.set('ready');
      },
      error: () => this.state.set('unavailable')
    });
  }
}
