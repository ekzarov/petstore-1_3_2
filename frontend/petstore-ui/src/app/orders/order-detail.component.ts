import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { OrderApiService } from './order-api.service';
import { Order } from './order.models';
import { LoadingStateComponent } from '../shared/loading-state.component';
import { UnavailableStateComponent } from '../shared/unavailable-state.component';

type DetailState = 'loading' | 'ready' | 'notFound' | 'unavailable';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [RouterLink, LoadingStateComponent, UnavailableStateComponent],
  template: `
    <p class="breadcrumb"><a routerLink="/orders">Back to orders</a></p>

    @if (state() === 'loading') {
      <app-loading-state />
    } @else if (state() === 'unavailable') {
      <app-unavailable-state />
    } @else if (state() === 'notFound') {
      <div class="state-not-found">
        <p>Order "{{ orderId() }}" was not found.</p>
        <a routerLink="/orders">Back to your orders</a>
      </div>
    } @else if (order(); as o) {
      <article class="order-detail">
        <h1 class="cart-title">
          Order #{{ o.orderId }}
          <span class="order-status" [class]="statusClass(o.status)">{{ o.status }}</span>
        </h1>
        <p class="identity-form__hint">Placed {{ o.placedAt.slice(0, 10) }}</p>

        <table class="cart-table">
          <thead>
            <tr><th>Item</th><th>Price</th><th>Qty</th><th>Subtotal</th></tr>
          </thead>
          <tbody>
            @for (line of o.lines; track line.itemId) {
              <tr>
                <td>
                  <a [routerLink]="['/catalog', 'items', line.itemId]">{{ line.name }}</a>
                  <span class="cart-table__id">{{ line.itemId }}</span>
                </td>
                <td>{{ line.unitPrice }} {{ line.currency }}</td>
                <td>{{ line.quantity }}</td>
                <td>{{ line.subtotal }} {{ line.currency }}</td>
              </tr>
            }
          </tbody>
        </table>

        <p class="cart-summary__total">Total: <strong>{{ o.total }} {{ o.currency }}</strong></p>

        <section class="order-detail__contact">
          <h2>Shipping to</h2>
          <p>
            {{ o.shippingContact.givenName }} {{ o.shippingContact.familyName }}<br />
            {{ o.shippingContact.street1 }}@if (o.shippingContact.street2) {, {{ o.shippingContact.street2 }}}<br />
            {{ o.shippingContact.city }}, {{ o.shippingContact.state }} {{ o.shippingContact.zip }},
            {{ o.shippingContact.country }}<br />
            {{ o.shippingContact.email }} · {{ o.shippingContact.phone }}
          </p>
        </section>
      </article>
    }
  `
})
export class OrderDetailComponent implements OnInit {
  private readonly orderApi = inject(OrderApiService);
  private readonly route = inject(ActivatedRoute);

  readonly state = signal<DetailState>('loading');
  readonly order = signal<Order | null>(null);
  readonly orderId = signal('');

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const orderId = params.get('orderId') ?? '';
      this.orderId.set(orderId);
      this.load(orderId);
    });
  }

  statusClass(status: string): string {
    return `order-status order-status--${status.toLowerCase().replace('_', '-')}`;
  }

  private load(orderId: string): void {
    this.state.set('loading');
    this.orderApi.getOrder(orderId).subscribe({
      next: (order) => {
        this.order.set(order);
        this.state.set('ready');
      },
      error: (err: unknown) => {
        this.state.set(
          err instanceof HttpErrorResponse && err.status === 404 ? 'notFound' : 'unavailable'
        );
      }
    });
  }
}
