import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { IdentityService } from '../identity/identity.service';

// Supplier operations: supplier role, admin retained as superuser (013/014 DP decisions).
export const supplierGuard: CanActivateFn = (_route, state) => {
  const identity = inject(IdentityService);
  const router = inject(Router);

  if (identity.isSupplier() || identity.isAdmin()) {
    return true;
  }

  return identity.isSignedIn()
    ? router.createUrlTree(['/catalog'])
    : router.createUrlTree(['/signin'], { queryParams: { returnUrl: state.url } });
};
