import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { RegisterRequest, SignInResponse, Account } from './identity.models';

const STORAGE_KEY = 'petstore.token';

@Injectable({ providedIn: 'root' })
export class IdentityService {
  private readonly http = inject(HttpClient);

  private readonly identity = signal<SignInResponse | null>(restoreIdentity());

  readonly userId = computed(() => this.identity()?.userId ?? null);
  readonly role = computed(() => this.identity()?.role ?? null);
  readonly isSignedIn = computed(() => this.identity() !== null);
  readonly isAdmin = computed(() => this.identity()?.role === 'admin');
  readonly isSupplier = computed(() => this.identity()?.role === 'supplier');

  get token(): string | null {
    return this.identity()?.token ?? null;
  }

  signIn(userId: string, password: string): Observable<SignInResponse> {
    return this.http
      .post<SignInResponse>('/api/auth/signin', { userId, password })
      .pipe(tap((response) => this.store(response)));
  }

  register(request: RegisterRequest): Observable<Account> {
    return this.http.post<Account>('/api/account', request);
  }

  signOut(): void {
    this.identity.set(null);
    localStorage.removeItem(STORAGE_KEY);
  }

  private store(response: SignInResponse): void {
    this.identity.set(response);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(response));
  }
}

function restoreIdentity(): SignInResponse | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }

    const stored = JSON.parse(raw) as SignInResponse;
    if (!stored.token || new Date(stored.expiresAt) <= new Date()) {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }

    return stored;
  } catch {
    localStorage.removeItem(STORAGE_KEY);
    return null;
  }
}
