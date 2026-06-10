import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartService } from './cart.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [RouterLink],
  template: `
    <h1 class="cart-title">Your cart</h1>

    @if (cart().lines.length === 0) {
      <div class="state-empty">
        <p>Your cart is empty.</p>
        <a routerLink="/catalog">Browse the catalog</a>
      </div>
    } @else {
      @if (error()) {
        <p class="cart-error">{{ error() }}</p>
      }

      <table class="cart-table">
        <thead>
          <tr>
            <th>Item</th>
            <th>Price</th>
            <th>Quantity</th>
            <th>Subtotal</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          @for (line of cart().lines; track line.itemId) {
            <tr [class.cart-table__row--unavailable]="line.unavailable">
              <td>
                <a [routerLink]="['/catalog', 'items', line.itemId]">{{ line.name }}</a>
                <span class="cart-table__id">{{ line.itemId }}</span>
                @if (line.unavailable) {
                  <span class="cart-table__flag">No longer available</span>
                }
              </td>
              <td>{{ line.unitPrice }} {{ line.currency }}</td>
              <td>
                <div class="cart-stepper">
                  <button type="button" (click)="setQuantity(line.itemId, line.quantity - 1)" [disabled]="busy()">−</button>
                  <span>{{ line.quantity }}</span>
                  <button type="button" (click)="setQuantity(line.itemId, line.quantity + 1)" [disabled]="busy() || line.quantity >= 99">+</button>
                </div>
              </td>
              <td>{{ line.subtotal }} {{ line.currency }}</td>
              <td>
                <button type="button" class="cart-remove" (click)="remove(line.itemId)" [disabled]="busy()">Remove</button>
              </td>
            </tr>
          }
        </tbody>
      </table>

      <div class="cart-summary">
        <p class="cart-summary__total">
          Total ({{ cart().itemCount }} items): <strong>{{ cart().total }} {{ cart().currency }}</strong>
        </p>
        <div class="cart-summary__actions">
          <button type="button" class="cart-empty-btn" (click)="empty()" [disabled]="busy()">Empty cart</button>
          <button type="button" class="cart-checkout-btn" disabled title="Checkout arrives in a later feature">Checkout</button>
        </div>
      </div>
    }
  `
})
export class CartComponent implements OnInit {
  private readonly cartService = inject(CartService);

  readonly cart = this.cartService.cart;
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.cartService.load();
  }

  setQuantity(itemId: string, quantity: number): void {
    this.run(this.cartService.setQuantity(itemId, Math.max(0, quantity)));
  }

  remove(itemId: string): void {
    this.run(this.cartService.removeLine(itemId));
  }

  empty(): void {
    this.run(this.cartService.empty());
  }

  private run(operation: { subscribe: (observer: { next: () => void; error: () => void }) => unknown }): void {
    if (this.busy()) {
      return;
    }

    this.busy.set(true);
    this.error.set(null);
    operation.subscribe({
      next: () => this.busy.set(false),
      error: () => {
        this.busy.set(false);
        this.error.set('The cart is currently unavailable. Please try again.');
      }
    });
  }
}
