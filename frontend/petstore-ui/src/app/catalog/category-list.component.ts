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
      <nav class="category-nav" aria-label="Pet categories">
        <ul class="category-list">
          @for (cat of state().data; track cat.id) {
            <li class="category-list__item">
              <a
                class="category-list__link"
                [routerLink]="['/catalog', 'categories', cat.id]"
                routerLinkActive="category-list__link--active"
              >{{ cat.name }}</a>
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
}
