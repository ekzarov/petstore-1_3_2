import { Component, inject } from '@angular/core';
import { Router, RouterOutlet, RouterLink } from '@angular/router';
import { IdentityService } from '../identity/identity.service';

@Component({
  selector: 'app-catalog-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  template: `
    <header class="catalog-header">
      <a class="catalog-header__brand" routerLink="/catalog">PetStore Catalog</a>
      <nav class="catalog-header__identity">
        @if (identity.isSignedIn()) {
          <a routerLink="/account" class="catalog-header__user">{{ identity.userId() }}</a>
          <button type="button" class="catalog-header__signout" (click)="signOut()">Sign out</button>
        } @else {
          <a routerLink="/signin" class="catalog-header__signin">Sign in</a>
        }
      </nav>
    </header>
    <main class="catalog-main">
      <router-outlet />
    </main>
  `
})
export class CatalogShellComponent {
  readonly identity = inject(IdentityService);
  private readonly router = inject(Router);

  signOut(): void {
    this.identity.signOut();
    this.router.navigateByUrl('/catalog');
  }
}
