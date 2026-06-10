import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CatalogApiService, CatalogNotFoundError } from './catalog-api.service';
import { CatalogItem, CatalogViewState } from './catalog.models';
import { CartService } from '../cart/cart.service';
import { LoadingStateComponent } from '../shared/loading-state.component';
import { UnavailableStateComponent } from '../shared/unavailable-state.component';

@Component({
  selector: 'app-item-detail',
  standalone: true,
  imports: [RouterLink, LoadingStateComponent, UnavailableStateComponent],
  template: `
    @if (state().status === 'loading') {
      <app-loading-state />
    } @else if (state().status === 'unavailable') {
      <app-unavailable-state />
    } @else if (state().status === 'notFound') {
      <div class="state-not-found">
        <p>Item "{{ itemId() }}" was not found.</p>
        <a routerLink="/catalog">Back to all categories</a>
      </div>
    } @else if (state().status === 'ready' && state().data; as item) {
      <p class="breadcrumb">
        <a [routerLink]="['/catalog', 'products', item.productId]">Back to items</a>
      </p>
      <article class="item-detail">
        <h1 class="item-detail__name">{{ item.name }}</h1>
        <dl class="item-detail__fields">
          <dt>Item id</dt>
          <dd>{{ item.id }}</dd>
          <dt>Product id</dt>
          <dd>{{ item.productId }}</dd>
          <dt>Attributes</dt>
          <dd>
            @if (item.attributes && item.attributes.length > 0) {
              {{ item.attributes.join(', ') }}
            } @else {
              <span class="item-detail__absent">None</span>
            }
          </dd>
          <dt>Description</dt>
          <dd>
            @if (item.description) {
              {{ item.description }}
            } @else {
              <span class="item-detail__absent">No description</span>
            }
          </dd>
          <dt>Price</dt>
          <dd class="item-detail__price">{{ item.price }} {{ item.currency }}</dd>
        </dl>
        <div class="add-to-cart">
          <button type="button" class="add-to-cart__btn" (click)="addToCart(item.id)" [disabled]="adding()">
            Add to cart
          </button>
          @if (added()) {
            <span class="add-to-cart__confirm">Added to cart</span>
          }
          @if (addError()) {
            <span class="add-to-cart__error">{{ addError() }}</span>
          }
        </div>
      </article>
    }
  `
})
export class ItemDetailComponent implements OnInit {
  private readonly api = inject(CatalogApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly cartService = inject(CartService);

  readonly adding = signal(false);
  readonly added = signal(false);
  readonly addError = signal<string | null>(null);

  addToCart(itemId: string): void {
    if (this.adding()) {
      return;
    }

    this.adding.set(true);
    this.added.set(false);
    this.addError.set(null);
    this.cartService.addItem(itemId).subscribe({
      next: () => {
        this.adding.set(false);
        this.added.set(true);
      },
      error: () => {
        this.adding.set(false);
        this.addError.set('Could not add to cart. Please try again.');
      }
    });
  }

  readonly itemId = signal('');
  readonly state = signal<CatalogViewState<CatalogItem>>({ status: 'loading' });

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const itemId = params.get('itemId') ?? '';
      this.itemId.set(itemId);
      this.load(itemId);
    });
  }

  private load(itemId: string): void {
    this.state.set({ status: 'loading' });
    this.api.getItemById(itemId).subscribe({
      next: (item) => this.state.set({ status: 'ready', data: item }),
      error: (err) =>
        this.state.set({ status: err instanceof CatalogNotFoundError ? 'notFound' : 'unavailable' })
    });
  }
}
