import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CatalogApiService, CatalogNotFoundError } from './catalog-api.service';
import { CatalogProduct, CatalogViewState } from './catalog.models';
import { CategoryListComponent } from './category-list.component';
import { LoadingStateComponent } from '../shared/loading-state.component';
import { EmptyStateComponent } from '../shared/empty-state.component';
import { UnavailableStateComponent } from '../shared/unavailable-state.component';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    RouterLink,
    CategoryListComponent,
    LoadingStateComponent,
    EmptyStateComponent,
    UnavailableStateComponent
  ],
  template: `
    <app-category-list />

    @if (state().status === 'loading') {
      <app-loading-state />
    } @else if (state().status === 'unavailable') {
      <app-unavailable-state />
    } @else if (state().status === 'notFound') {
      <div class="state-not-found">
        <p>Category "{{ categoryId() }}" was not found.</p>
        <a routerLink="/catalog">Back to all categories</a>
      </div>
    } @else if (state().status === 'empty') {
      <app-empty-state message="No products in this category yet." />
    } @else if (state().status === 'ready') {
      <ul class="product-list">
        @for (product of state().data; track product.id) {
          <li class="product-card">
            <a
              class="product-card__link"
              [routerLink]="['/catalog', 'products', product.id]"
              [queryParams]="{ category: product.categoryId }"
            >
              <span class="product-card__name">{{ product.name }}</span>
              @if (product.description) {
                <span class="product-card__description">{{ product.description }}</span>
              }
            </a>
          </li>
        }
      </ul>
    }
  `
})
export class ProductListComponent implements OnInit {
  private readonly api = inject(CatalogApiService);
  private readonly route = inject(ActivatedRoute);

  readonly categoryId = signal('');
  readonly state = signal<CatalogViewState<CatalogProduct[]>>({ status: 'loading' });

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const categoryId = params.get('categoryId') ?? '';
      this.categoryId.set(categoryId);
      this.load(categoryId);
    });
  }

  private load(categoryId: string): void {
    this.state.set({ status: 'loading' });
    this.api.getProductsByCategory(categoryId).subscribe({
      next: (products) =>
        this.state.set(
          products.length === 0 ? { status: 'empty' } : { status: 'ready', data: products }
        ),
      error: (err) =>
        this.state.set({ status: err instanceof CatalogNotFoundError ? 'notFound' : 'unavailable' })
    });
  }
}
