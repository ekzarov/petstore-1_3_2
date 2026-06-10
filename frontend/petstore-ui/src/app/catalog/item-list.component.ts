import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CatalogApiService, CatalogNotFoundError } from './catalog-api.service';
import { CatalogItem, CatalogViewState } from './catalog.models';
import { CartService } from '../cart/cart.service';
import { LoadingStateComponent } from '../shared/loading-state.component';
import { EmptyStateComponent } from '../shared/empty-state.component';
import { UnavailableStateComponent } from '../shared/unavailable-state.component';

@Component({
  selector: 'app-item-list',
  standalone: true,
  imports: [RouterLink, LoadingStateComponent, EmptyStateComponent, UnavailableStateComponent],
  template: `
    <p class="breadcrumb">
      @if (categoryId()) {
        <a [routerLink]="['/catalog', 'categories', categoryId()]">Back to products</a>
      } @else {
        <a routerLink="/catalog">Back to categories</a>
      }
    </p>

    @if (state().status === 'loading') {
      <app-loading-state />
    } @else if (state().status === 'unavailable') {
      <app-unavailable-state />
    } @else if (state().status === 'notFound') {
      <div class="state-not-found">
        <p>Product "{{ productId() }}" was not found.</p>
        <a routerLink="/catalog">Back to all categories</a>
      </div>
    } @else if (state().status === 'empty') {
      <app-empty-state message="No items available for this product yet." />
    } @else if (state().status === 'ready') {
      <ul class="item-list">
        @for (item of state().data; track item.id) {
          <li class="item-card">
            <a
              class="item-card__link"
              [routerLink]="['/catalog', 'items', item.id]"
            >
              <span class="item-card__name">{{ item.name }}</span>
              <span class="item-card__id">{{ item.id }}</span>
              <span class="item-card__price">{{ item.price }} {{ item.currency }}</span>
            </a>
            <div class="item-card__actions">
              <button type="button" class="add-to-cart__btn" (click)="addToCart(item.id)" [disabled]="addingId() === item.id">
                Add to cart
              </button>
              @if (addedId() === item.id) {
                <span class="add-to-cart__confirm">Added</span>
              }
            </div>
          </li>
        }
      </ul>
    }
  `
})
export class ItemListComponent implements OnInit {
  private readonly api = inject(CatalogApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly cartService = inject(CartService);

  readonly addingId = signal<string | null>(null);
  readonly addedId = signal<string | null>(null);

  addToCart(itemId: string): void {
    if (this.addingId()) {
      return;
    }

    this.addingId.set(itemId);
    this.cartService.addItem(itemId).subscribe({
      next: () => {
        this.addingId.set(null);
        this.addedId.set(itemId);
      },
      error: () => this.addingId.set(null)
    });
  }

  readonly productId = signal('');
  readonly categoryId = signal('');
  readonly state = signal<CatalogViewState<CatalogItem[]>>({ status: 'loading' });

  ngOnInit(): void {
    this.route.queryParamMap.subscribe((query) => {
      this.categoryId.set(query.get('category') ?? '');
    });
    this.route.paramMap.subscribe((params) => {
      const productId = params.get('productId') ?? '';
      this.productId.set(productId);
      this.load(productId);
    });
  }

  private load(productId: string): void {
    this.state.set({ status: 'loading' });
    this.api.getItemsByProduct(productId).subscribe({
      next: (items) =>
        this.state.set(
          items.length === 0 ? { status: 'empty' } : { status: 'ready', data: items }
        ),
      error: (err) =>
        this.state.set({ status: err instanceof CatalogNotFoundError ? 'notFound' : 'unavailable' })
    });
  }
}
