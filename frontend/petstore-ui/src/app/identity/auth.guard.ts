import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { IdentityService } from './identity.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const identity = inject(IdentityService);
  const router = inject(Router);

  if (identity.isSignedIn()) {
    return true;
  }

  return router.createUrlTree(['/signin'], { queryParams: { returnUrl: state.url } });
};
