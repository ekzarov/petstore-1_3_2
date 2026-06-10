import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';

@Component({
  selector: 'app-catalog-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <header class="catalog-header">
      <a class="catalog-header__brand" routerLink="/catalog">PetStore Catalog</a>
    </header>
    <main class="catalog-main">
      <router-outlet />
    </main>
  `
})
export class CatalogShellComponent {}
