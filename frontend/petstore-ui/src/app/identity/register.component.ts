import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { IdentityService } from './identity.service';
import { ApiError, ContactInfo } from './identity.models';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterLink],
  template: `
    <form class="identity-form identity-form--wide" (ngSubmit)="submit()">
      <h1>Create an account</h1>

      @if (error()) {
        <p class="identity-form__error">{{ error() }}</p>
      }

      <fieldset>
        <legend>Sign-in details</legend>
        <label>
          User id
          <input name="userId" [(ngModel)]="userId" autocomplete="username" required />
        </label>
        <label>
          Password
          <input name="password" type="password" [(ngModel)]="password" autocomplete="new-password" required />
        </label>
      </fieldset>

      <fieldset>
        <legend>Contact information</legend>
        <label>Family name <input name="familyName" [(ngModel)]="contact.familyName" required /></label>
        <label>Given name <input name="givenName" [(ngModel)]="contact.givenName" required /></label>
        <label>Street <input name="street1" [(ngModel)]="contact.street1" required /></label>
        <label>Street (line 2) <input name="street2" [(ngModel)]="contact.street2" /></label>
        <label>City <input name="city" [(ngModel)]="contact.city" required /></label>
        <label>State <input name="state" [(ngModel)]="contact.state" required /></label>
        <label>Zip <input name="zip" [(ngModel)]="contact.zip" required /></label>
        <label>Country <input name="country" [(ngModel)]="contact.country" required /></label>
        <label>Email <input name="email" type="email" [(ngModel)]="contact.email" required /></label>
        <label>Phone <input name="phone" [(ngModel)]="contact.phone" required /></label>
      </fieldset>

      <button type="submit" [disabled]="submitting()">Create account</button>

      <p class="identity-form__hint">
        Already have an account? <a routerLink="/signin">Sign in</a>
      </p>
    </form>
  `
})
export class RegisterComponent {
  private readonly identity = inject(IdentityService);
  private readonly router = inject(Router);

  userId = '';
  password = '';
  contact: ContactInfo = {
    familyName: '',
    givenName: '',
    street1: '',
    street2: '',
    city: '',
    state: '',
    zip: '',
    country: '',
    email: '',
    phone: ''
  };

  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  submit(): void {
    if (this.submitting()) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.identity
      .register({ userId: this.userId, password: this.password, contact: this.contact })
      .subscribe({
        next: () => {
          // Auto sign-in after successful registration (plan DD-004).
          this.identity.signIn(this.userId, this.password).subscribe({
            next: () => this.router.navigateByUrl('/catalog'),
            error: () => this.router.navigateByUrl('/signin')
          });
        },
        error: (err: unknown) => {
          this.submitting.set(false);
          this.error.set(toMessage(err));
        }
      });
  }
}

function toMessage(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 409) {
      return 'An account with this user id already exists.';
    }

    if (err.status === 400 && err.error) {
      return (err.error as ApiError).message;
    }
  }

  return 'Registration is currently unavailable. Please try again later.';
}
