import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Cart } from './cart.models';
import { IdentityService } from '../identity/identity.service';

const EMPTY_CART: Cart = { lines: [], itemCount: 0, total: 0, currency: 'USD' };

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly http = inject(HttpClient);
  private readonly identity = inject(IdentityService);

  private readonly cartState = signal<Cart>(EMPTY_CART);

  readonly cart = this.cartState.asReadonly();
  // Header badge counts distinct cart lines, not total units.
  readonly itemCount = computed(() => this.cartState().lines.filter((line) => !line.unavailable).length);

  constructor() {
    // Reload the cart whenever the signed-in identity changes so the
    // backend merge of an anonymous cart becomes visible immediately.
    effect(() => {
      this.identity.userId();
      this.load();
    });
  }

  load(): void {
    this.http.get<Cart>('/api/cart').subscribe({
      next: (cart) => this.cartState.set(cart),
      error: () => this.cartState.set(EMPTY_CART)
    });
  }

  addItem(itemId: string): Observable<Cart> {
    return this.http
      .post<Cart>('/api/cart/items', { itemId })
      .pipe(tap((cart) => this.cartState.set(cart)));
  }

  setQuantity(itemId: string, quantity: number): Observable<Cart> {
    return this.http
      .put<Cart>(`/api/cart/items/${itemId}`, { quantity })
      .pipe(tap((cart) => this.cartState.set(cart)));
  }

  removeLine(itemId: string): Observable<Cart> {
    return this.http
      .delete<Cart>(`/api/cart/items/${itemId}`)
      .pipe(tap((cart) => this.cartState.set(cart)));
  }

  empty(): Observable<Cart> {
    return this.http
      .delete<Cart>('/api/cart')
      .pipe(tap((cart) => this.cartState.set(cart)));
  }
}
