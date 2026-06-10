import { Routes } from '@angular/router';
import { CatalogShellComponent } from './catalog/catalog-shell.component';

export const routes: Routes = [
  {
    path: 'catalog',
    component: CatalogShellComponent,
    children: [
      // T015: category list wired here in Phase 3
      // T019: category products route wired here in Phase 4
      // T024: product items and item detail routes wired here in Phase 5
    ]
  },
  { path: '', redirectTo: '/catalog', pathMatch: 'full' }
];
