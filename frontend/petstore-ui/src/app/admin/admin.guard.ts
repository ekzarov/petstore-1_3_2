import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { IdentityService } from '../identity/identity.service';

export const adminGuard: CanActivateFn = (_route, state) => {
  const identity = inject(IdentityService);
  const router = inject(Router);

  if (identity.isAdmin()) {
    return true;
  }

  // Anonymous users go to sign-in; signed-in customers go back to the catalog.
  return identity.isSignedIn()
    ? router.createUrlTree(['/catalog'])
    : router.createUrlTree(['/signin'], { queryParams: { returnUrl: state.url } });
};
