import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { CatalogCategory, CatalogProduct, CatalogItem } from './catalog.models';

export class CatalogNotFoundError extends Error {}
export class CatalogUnavailableError extends Error {}

@Injectable({ providedIn: 'root' })
export class CatalogApiService {
  private readonly http = inject(HttpClient);

  getCategories(): Observable<CatalogCategory[]> {
    return this.http
      .get<CatalogCategory[]>('/api/catalog/categories')
      .pipe(catchError(this.handleError));
  }

  getProductsByCategory(categoryId: string): Observable<CatalogProduct[]> {
    return this.http
      .get<CatalogProduct[]>(`/api/catalog/categories/${categoryId}/products`)
      .pipe(catchError(this.handleError));
  }

  getItemsByProduct(productId: string): Observable<CatalogItem[]> {
    return this.http
      .get<CatalogItem[]>(`/api/catalog/products/${productId}/items`)
      .pipe(catchError(this.handleError));
  }

  getItemById(itemId: string): Observable<CatalogItem> {
    return this.http
      .get<CatalogItem>(`/api/catalog/items/${itemId}`)
      .pipe(catchError(this.handleError));
  }

  private handleError(err: HttpErrorResponse): Observable<never> {
    if (err.status === 404) {
      return throwError(() => new CatalogNotFoundError());
    }
    return throwError(() => new CatalogUnavailableError());
  }
}
