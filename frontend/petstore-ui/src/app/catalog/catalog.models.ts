export interface CatalogCategory {
  id: string;
  name: string;
  description?: string | null;
}

export interface CatalogProduct {
  id: string;
  categoryId: string;
  name: string;
  description?: string | null;
}

export interface CatalogItem {
  id: string;
  productId: string;
  name: string;
  attributes?: string[];
  description?: string | null;
  price: number;
  currency: string;
}

export type ViewStatus = 'loading' | 'ready' | 'empty' | 'notFound' | 'unavailable';

export interface CatalogViewState<T> {
  status: ViewStatus;
  data?: T;
}
