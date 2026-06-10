import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { IdentityService } from '../identity/identity.service';
import { Account, ApiError, ContactInfo } from '../identity/identity.models';
import { LoadingStateComponent } from '../shared/loading-state.component';
import { UnavailableStateComponent } from '../shared/unavailable-state.component';

type AccountState = 'loading' | 'ready' | 'unavailable';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [FormsModule, LoadingStateComponent, UnavailableStateComponent],
  template: `
    @if (state() === 'loading') {
      <app-loading-state />
    } @else if (state() === 'unavailable') {
      <app-unavailable-state />
    } @else {
      <form class="identity-form identity-form--wide" (ngSubmit)="save()">
        <h1>My account</h1>
        <p class="identity-form__hint">Signed in as <strong>{{ identity.userId() }}</strong></p>

        @if (error()) {
          <p class="identity-form__error">{{ error() }}</p>
        }
        @if (saved()) {
          <p class="identity-form__success">Contact details saved.</p>
        }

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

        <button type="submit" [disabled]="saving()">Save</button>
      </form>
    }
  `
})
export class AccountComponent implements OnInit {
  private readonly http = inject(HttpClient);
  readonly identity = inject(IdentityService);

  readonly state = signal<AccountState>('loading');
  readonly saving = signal(false);
  readonly saved = signal(false);
  readonly error = signal<string | null>(null);

  contact: ContactInfo = emptyContact();

  ngOnInit(): void {
    this.http.get<Account>('/api/account').subscribe({
      next: (account) => {
        this.contact = account.contact ?? emptyContact();
        this.state.set('ready');
      },
      error: () => this.state.set('unavailable')
    });
  }

  save(): void {
    if (this.saving()) {
      return;
    }

    this.saving.set(true);
    this.saved.set(false);
    this.error.set(null);
    this.http.put<Account>('/api/account/contact', this.contact).subscribe({
      next: (account) => {
        this.contact = account.contact ?? this.contact;
        this.saving.set(false);
        this.saved.set(true);
      },
      error: (err: unknown) => {
        this.saving.set(false);
        this.error.set(
          err instanceof HttpErrorResponse && err.status === 400 && err.error
            ? (err.error as ApiError).message
            : 'Saving is currently unavailable. Please try again later.'
        );
      }
    });
  }
}

function emptyContact(): ContactInfo {
  return {
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
}
