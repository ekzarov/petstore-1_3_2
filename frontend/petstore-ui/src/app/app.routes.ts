import { Routes } from '@angular/router';
import { CatalogShellComponent } from './catalog/catalog-shell.component';
import { CategoryListComponent } from './catalog/category-list.component';
import { ProductListComponent } from './catalog/product-list.component';
import { ItemListComponent } from './catalog/item-list.component';
import { ItemDetailComponent } from './catalog/item-detail.component';

export const routes: Routes = [
  {
    path: 'catalog',
    component: CatalogShellComponent,
    children: [
      { path: '', component: CategoryListComponent },
      { path: 'categories/:categoryId', component: ProductListComponent },
      { path: 'products/:productId', component: ItemListComponent },
      { path: 'items/:itemId', component: ItemDetailComponent }
    ]
  },
  { path: '', redirectTo: '/catalog', pathMatch: 'full' }
];
