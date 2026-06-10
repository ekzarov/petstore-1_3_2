import { Routes } from '@angular/router';
import { CatalogShellComponent } from './catalog/catalog-shell.component';
import { CategoryListComponent } from './catalog/category-list.component';
import { ProductListComponent } from './catalog/product-list.component';
import { ItemListComponent } from './catalog/item-list.component';
import { ItemDetailComponent } from './catalog/item-detail.component';
import { SignInComponent } from './identity/sign-in.component';
import { RegisterComponent } from './identity/register.component';
import { AccountComponent } from './account/account.component';
import { authGuard } from './identity/auth.guard';

export const routes: Routes = [
  {
    path: '',
    component: CatalogShellComponent,
    children: [
      { path: '', redirectTo: 'catalog', pathMatch: 'full' },
      {
        path: 'catalog',
        children: [
          { path: '', component: CategoryListComponent },
          { path: 'categories/:categoryId', component: ProductListComponent },
          { path: 'products/:productId', component: ItemListComponent },
          { path: 'items/:itemId', component: ItemDetailComponent }
        ]
      },
      { path: 'signin', component: SignInComponent },
      { path: 'register', component: RegisterComponent },
      { path: 'account', component: AccountComponent, canActivate: [authGuard] }
    ]
  }
];
