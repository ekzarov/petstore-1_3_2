import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { CatalogApiService } from './catalog-api.service';
import { CatalogCategory, CatalogViewState } from './catalog.models';
import { LoadingStateComponent } from '../shared/loading-state.component';
import { UnavailableStateComponent } from '../shared/unavailable-state.component';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, LoadingStateComponent, UnavailableStateComponent],
  template: `
    @if (state().status === 'loading') {
      <app-loading-state />
    } @else if (state().status === 'unavailable') {
      <app-unavailable-state />
    } @else if (state().status === 'ready') {
      <section class="category-hero" aria-labelledby="category-hero-title">
        <div class="category-hero__copy">
          <p class="category-hero__eyebrow">PetStore catalog</p>
          <h1 id="category-hero-title">Browse pets by category.</h1>
          <p>
            Pick a lane below to open the product list backed by the catalog API.
          </p>
        </div>
        <img
          class="category-hero__image"
          src="catalog-category-icons.png"
          alt="Glossy icons for fish, dogs, reptiles, cats, and birds"
        />
      </section>

      <nav class="category-nav category-nav--cards" aria-label="Pet categories">
        <ul class="category-list">
          @for (cat of state().data; track cat.id) {
            <li class="category-list__item">
              <a
                class="category-list__link"
                [routerLink]="['/catalog', 'categories', cat.id]"
                routerLinkActive="category-list__link--active"
                [style.--accent]="categoryAccent(cat.id)"
                [attr.aria-label]="'Open ' + cat.name + ' products'"
              >
                <span class="category-list__mark">{{ categoryMark(cat.id) }}</span>
                <span class="category-list__body">
                  <span class="category-list__name">{{ cat.name }}</span>
                  <span class="category-list__description">
                    {{ categorySummary(cat) }}
                  </span>
                  <span class="category-list__action">View products</span>
                </span>
              </a>
            </li>
          }
        </ul>
      </nav>
    }
  `
})
export class CategoryListComponent implements OnInit {
  private readonly api = inject(CatalogApiService);

  readonly state = signal<CatalogViewState<CatalogCategory[]>>({ status: 'loading' });

  ngOnInit(): void {
    this.api.getCategories().subscribe({
      next: (cats) => this.state.set({ status: 'ready', data: cats }),
      error: () => this.state.set({ status: 'unavailable' })
    });
  }

  categoryAccent(categoryId: string): string {
    return (
      {
        FISH: '#14a9b8',
        DOGS: '#ef765f',
        REPTILES: '#8dae32',
        CATS: '#e7a12f',
        BIRDS: '#529bd5'
      }[categoryId.toUpperCase()] ?? '#0057b8'
    );
  }

  categoryMark(categoryId: string): string {
    return categoryId.slice(0, 2).toUpperCase();
  }

  categorySummary(category: CatalogCategory): string {
    if (category.description) {
      return category.description;
    }

    return (
      {
        FISH: 'Aquatic companions and bright tank favorites.',
        DOGS: 'Friendly classics for the most loyal aisle.',
        REPTILES: 'Calm, curious pets with sun-warmed style.',
        CATS: 'Soft, sharp, and fully in charge of the room.',
        BIRDS: 'Light, colorful companions with a little song.'
      }[category.id.toUpperCase()] ?? 'Open this category to browse products.'
    );
  }
}
