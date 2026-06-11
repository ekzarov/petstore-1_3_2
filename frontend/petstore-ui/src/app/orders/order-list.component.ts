import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OrderApiService } from './order-api.service';
import { OrderSummary } from './order.models';
import { LoadingStateComponent } from '../shared/loading-state.component';
import { UnavailableStateComponent } from '../shared/unavailable-state.component';

type ListState = 'loading' | 'ready' | 'unavailable';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [RouterLink, LoadingStateComponent, UnavailableStateComponent],
  template: `
    <h1 class="cart-title">Your orders</h1>

    @if (state() === 'loading') {
      <app-loading-state />
    } @else if (state() === 'unavailable') {
      <app-unavailable-state />
    } @else if (orders().length === 0) {
      <div class="state-empty">
        <p>You have no orders yet.</p>
        <a routerLink="/catalog">Browse the catalog</a>
      </div>
    } @else {
      <table class="cart-table">
        <thead>
          <tr><th>Order</th><th>Placed</th><th>Total</th><th>Status</th></tr>
        </thead>
        <tbody>
          @for (order of orders(); track order.orderId) {
            <tr>
              <td><a [routerLink]="['/orders', order.orderId]">#{{ order.orderId }}</a></td>
              <td>{{ order.placedAt.slice(0, 10) }}</td>
              <td>{{ order.total }} {{ order.currency }}</td>
              <td><span class="order-status" [class]="statusClass(order.status)">{{ order.status }}</span></td>
            </tr>
          }
        </tbody>
      </table>
    }
  `
})
export class OrderListComponent implements OnInit {
  private readonly orderApi = inject(OrderApiService);

  readonly state = signal<ListState>('loading');
  readonly orders = signal<OrderSummary[]>([]);

  ngOnInit(): void {
    this.orderApi.getOrders().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.state.set('ready');
      },
      error: () => this.state.set('unavailable')
    });
  }

  statusClass(status: string): string {
    return `order-status order-status--${status.toLowerCase().replace('_', '-')}`;
  }
}
