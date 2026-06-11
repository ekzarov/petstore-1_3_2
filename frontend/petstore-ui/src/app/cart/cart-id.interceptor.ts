import { HttpInterceptorFn } from '@angular/common/http';

const STORAGE_KEY = 'petstore.cartId';

export function getCartId(): string {
  let cartId = localStorage.getItem(STORAGE_KEY);
  if (!cartId) {
    cartId = crypto.randomUUID();
    localStorage.setItem(STORAGE_KEY, cartId);
  }

  return cartId;
}

export const cartIdInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.includes('/api/cart')) {
    return next(req);
  }

  return next(req.clone({ setHeaders: { 'X-Cart-Id': getCartId() } }));
};
