import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { IdentityService } from './identity.service';

@Component({
  selector: 'app-sign-in',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <form class="identity-form" (ngSubmit)="submit()">
      <h1>Sign in</h1>

      @if (error()) {
        <p class="identity-form__error">{{ error() }}</p>
      }

      <label>
        User id
        <input name="userId" [(ngModel)]="userId" autocomplete="username" required />
      </label>

      <label>
        Password
        <input name="password" type="password" [(ngModel)]="password" autocomplete="current-password" required />
      </label>

      <button type="submit" [disabled]="submitting()">Sign in</button>

      <p class="identity-form__hint">
        New to the store? <a routerLink="/register">Create an account</a>
      </p>
    </form>
  `
})
export class SignInComponent {
  private readonly identity = inject(IdentityService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  userId = '';
  password = '';
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  submit(): void {
    if (!this.userId || !this.password || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.identity.signIn(this.userId, this.password).subscribe({
      next: () => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') ?? '/catalog';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err: unknown) => {
        this.submitting.set(false);
        this.password = '';
        this.error.set(
          err instanceof HttpErrorResponse && err.status === 401
            ? 'User id or password is incorrect.'
            : 'Sign-in is currently unavailable. Please try again later.'
        );
      }
    });
  }
}
