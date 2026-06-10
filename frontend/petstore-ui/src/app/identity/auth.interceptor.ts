import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { IdentityService } from './identity.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const identity = inject(IdentityService);
  const router = inject(Router);

  const token = identity.token;
  const request = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(request).pipe(
    catchError((err: unknown) => {
      // A 401 on a non-sign-in call means the session expired or was invalidated.
      if (
        err instanceof HttpErrorResponse &&
        err.status === 401 &&
        !req.url.includes('/api/auth/')
      ) {
        identity.signOut();
        router.navigate(['/signin'], { queryParams: { returnUrl: router.url } });
      }

      return throwError(() => err);
    })
  );
};
