import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { AdminApiService } from './admin-api.service';
import { AdminOrderDetail, OrderTransition } from './admin.models';
import { LoadingStateComponent } from '../shared/loading-state.component';
import { UnavailableStateComponent } from '../shared/unavailable-state.component';

@Component({
  selector: 'app-admin-order-detail',
  standalone: true,
  imports: [RouterLink, LoadingStateComponent, UnavailableStateComponent],
  template: `
    <p class="breadcrumb"><a routerLink="/admin/orders">Back to all orders</a></p>

    @if (state() === 'loading') {
      <app-loading-state />
    } @else if (state() === 'unavailable') {
      <app-unavailable-state />
    } @else if (state() === 'notFound') {
      <div class="state-not-found">
        <p>Order "{{ orderId() }}" was not found.</p>
        <a routerLink="/admin/orders">Back to all orders</a>
      </div>
    } @else if (order(); as o) {
      <article class="order-detail">
        <h1 class="cart-title">
          Order #{{ o.orderId }}
          <span class="order-status" [class]="'order-status order-status--' + o.status.toLowerCase().replace('_', '-')">{{ o.status }}</span>
        </h1>
        <p class="identity-form__hint">Placed {{ o.placedAt.slice(0, 10) }} by <strong>{{ o.userId }}</strong></p>

        <table class="cart-table">
          <thead>
            <tr><th>Item</th><th>Price</th><th>Qty</th><th>Subtotal</th></tr>
          </thead>
          <tbody>
            @for (line of o.lines; track line.itemId) {
              <tr>
                <td>{{ line.name }} <span class="cart-table__id">{{ line.itemId }}</span></td>
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
            {{ o.shippingContact.givenName }} {{ o.shippingContact.familyName }},
            {{ o.shippingContact.street1 }}, {{ o.shippingContact.city }},
            {{ o.shippingContact.state }} {{ o.shippingContact.zip }}, {{ o.shippingContact.country }}<br />
            {{ o.shippingContact.email }} · {{ o.shippingContact.phone }}
          </p>
        </section>

        <section class="order-detail__contact">
          <h2>Status history</h2>
          @if (transitions().length === 0) {
            <p class="identity-form__hint">No transitions recorded yet.</p>
          } @else {
            <ul class="admin-transitions">
              @for (transition of transitions(); track transition.occurredAt) {
                <li>
                  <span class="order-status" [class]="'order-status order-status--' + transition.fromStatus.toLowerCase().replace('_', '-')">{{ transition.fromStatus }}</span>
                  →
                  <span class="order-status" [class]="'order-status order-status--' + transition.toStatus.toLowerCase().replace('_', '-')">{{ transition.toStatus }}</span>
                  <span class="admin-transitions__meta">by {{ transition.actor }} at {{ transition.occurredAt.slice(0, 19).replace('T', ' ') }}</span>
                </li>
              }
            </ul>
          }
        </section>
      </article>
    }
  `
})
export class AdminOrderDetailComponent implements OnInit {
  private readonly api = inject(AdminApiService);
  private readonly route = inject(ActivatedRoute);

  readonly state = signal<'loading' | 'ready' | 'notFound' | 'unavailable'>('loading');
  readonly order = signal<AdminOrderDetail | null>(null);
  readonly transitions = signal<OrderTransition[]>([]);
  readonly orderId = signal('');

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const orderId = params.get('orderId') ?? '';
      this.orderId.set(orderId);
      this.load(orderId);
    });
  }

  private load(orderId: string): void {
    this.state.set('loading');
    forkJoin({
      order: this.api.getOrder(orderId),
      transitions: this.api.getTransitions(orderId)
    }).subscribe({
      next: ({ order, transitions }) => {
        this.order.set(order);
        this.transitions.set(transitions);
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
