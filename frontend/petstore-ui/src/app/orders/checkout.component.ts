import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { CartService } from '../cart/cart.service';
import { OrderApiService } from './order-api.service';
import { Order } from './order.models';
import { Account, ApiError, ContactInfo } from '../identity/identity.models';
import { LoadingStateComponent } from '../shared/loading-state.component';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [FormsModule, RouterLink, LoadingStateComponent],
  template: `
    @if (loading()) {
      <app-loading-state />
    } @else if (placedOrder(); as order) {
      <div class="checkout-confirmation">
        <h1>Thank you for your order!</h1>
        <p>
          Order <strong>#{{ order.orderId }}</strong> was placed with status
          <span class="order-status order-status--pending">{{ order.status }}</span>.
        </p>
        <p class="checkout-confirmation__total">
          Total: <strong>{{ order.total }} {{ order.currency }}</strong>
        </p>
        <p>
          <a [routerLink]="['/orders', order.orderId]">View order details</a> ·
          <a routerLink="/orders">Order history</a> ·
          <a routerLink="/catalog">Continue shopping</a>
        </p>
      </div>
    } @else if (cart().lines.length === 0) {
      <div class="state-empty">
        <p>Your cart is empty — there is nothing to check out.</p>
        <a routerLink="/catalog">Browse the catalog</a>
      </div>
    } @else {
      <h1 class="cart-title">Checkout</h1>

      @if (error()) {
        <p class="cart-error">{{ error() }}</p>
      }

      <section class="checkout-review">
        <h2>Order review</h2>
        <table class="cart-table">
          <thead>
            <tr><th>Item</th><th>Price</th><th>Qty</th><th>Subtotal</th></tr>
          </thead>
          <tbody>
            @for (line of cart().lines; track line.itemId) {
              <tr>
                <td>{{ line.name }} <span class="cart-table__id">{{ line.itemId }}</span></td>
                <td>{{ line.unitPrice }} {{ line.currency }}</td>
                <td>{{ line.quantity }}</td>
                <td>{{ line.subtotal }} {{ line.currency }}</td>
              </tr>
            }
          </tbody>
        </table>
        <p class="cart-summary__total">
          Total ({{ cart().lines.length }} items, {{ cart().itemCount }} units):
          <strong>{{ cart().total }} {{ cart().currency }}</strong>
        </p>
        <p class="identity-form__hint"><a routerLink="/cart">Edit cart</a></p>
      </section>

      <form class="identity-form identity-form--wide" (ngSubmit)="placeOrder()">
        <fieldset>
          <legend>Shipping address</legend>
          <label>Family name <input name="familyName" [(ngModel)]="shipping.familyName" required /></label>
          <label>Given name <input name="givenName" [(ngModel)]="shipping.givenName" required /></label>
          <label>Street <input name="street1" [(ngModel)]="shipping.street1" required /></label>
          <label>Street (line 2) <input name="street2" [(ngModel)]="shipping.street2" /></label>
          <label>City <input name="city" [(ngModel)]="shipping.city" required /></label>
          <label>State <input name="state" [(ngModel)]="shipping.state" required /></label>
          <label>Zip <input name="zip" [(ngModel)]="shipping.zip" required /></label>
          <label>Country <input name="country" [(ngModel)]="shipping.country" required /></label>
          <label>Email <input name="email" type="email" [(ngModel)]="shipping.email" required /></label>
          <label>Phone <input name="phone" [(ngModel)]="shipping.phone" required /></label>
        </fieldset>

        <label class="checkout-billing-toggle">
          <input type="checkbox" name="sameBilling" [(ngModel)]="billingSameAsShipping" />
          Billing address is the same as shipping
        </label>

        @if (!billingSameAsShipping) {
          <fieldset>
            <legend>Billing address</legend>
            <label>Family name <input name="bFamilyName" [(ngModel)]="billing.familyName" required /></label>
            <label>Given name <input name="bGivenName" [(ngModel)]="billing.givenName" required /></label>
            <label>Street <input name="bStreet1" [(ngModel)]="billing.street1" required /></label>
            <label>Street (line 2) <input name="bStreet2" [(ngModel)]="billing.street2" /></label>
            <label>City <input name="bCity" [(ngModel)]="billing.city" required /></label>
            <label>State <input name="bState" [(ngModel)]="billing.state" required /></label>
            <label>Zip <input name="bZip" [(ngModel)]="billing.zip" required /></label>
            <label>Country <input name="bCountry" [(ngModel)]="billing.country" required /></label>
            <label>Email <input name="bEmail" type="email" [(ngModel)]="billing.email" required /></label>
            <label>Phone <input name="bPhone" [(ngModel)]="billing.phone" required /></label>
          </fieldset>
        }

        <button type="submit" [disabled]="submitting()">
          {{ submitting() ? 'Placing order…' : 'Place order' }}
        </button>
      </form>
    }
  `
})
export class CheckoutComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly cartService = inject(CartService);
  private readonly orderApi = inject(OrderApiService);

  readonly cart = this.cartService.cart;
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);
  readonly placedOrder = signal<Order | null>(null);

  shipping: ContactInfo = emptyContact();
  billing: ContactInfo = emptyContact();
  billingSameAsShipping = true;

  ngOnInit(): void {
    this.cartService.load();
    // Prefill the shipping block from the account contact details.
    this.http.get<Account>('/api/account').subscribe({
      next: (account) => {
        if (account.contact) {
          this.shipping = { ...account.contact };
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  placeOrder(): void {
    if (this.submitting()) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.orderApi
      .placeOrder({
        shippingContact: this.shipping,
        billingContact: this.billingSameAsShipping ? null : this.billing
      })
      .subscribe({
        next: (order) => {
          this.placedOrder.set(order);
          this.submitting.set(false);
          this.cartService.load();
        },
        error: (err: unknown) => {
          this.submitting.set(false);
          this.error.set(toMessage(err));
          this.cartService.load();
        }
      });
  }
}

function toMessage(err: unknown): string {
  if (err instanceof HttpErrorResponse && err.error && (err.error as ApiError).message) {
    return (err.error as ApiError).message;
  }

  return 'Order placement is currently unavailable. Please try again.';
}

function emptyContact(): ContactInfo {
  return {
    familyName: '',
    givenName: '',
    street1: '',
    street2: '',
    city: '',
    state: '',
    zip: '',
    country: '',
    email: '',
    phone: ''
  };
}
