import { Routes } from '@angular/router';
import { CatalogShellComponent } from './catalog/catalog-shell.component';
import { CategoryListComponent } from './catalog/category-list.component';

export const routes: Routes = [
  {
    path: 'catalog',
    component: CatalogShellComponent,
    children: [
      { path: '', component: CategoryListComponent },
      { path: 'categories/:categoryId', component: CategoryListComponent },
      // T019: product list route wired here in Phase 4
      // T024: product items and item detail routes wired here in Phase 5
    ]
  },
  { path: '', redirectTo: '/catalog', pathMatch: 'full' }
];
