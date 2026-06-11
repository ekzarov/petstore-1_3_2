import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order, OrderSummary, PlaceOrderRequest } from './order.models';

@Injectable({ providedIn: 'root' })
export class OrderApiService {
  private readonly http = inject(HttpClient);

  placeOrder(request: PlaceOrderRequest): Observable<Order> {
    return this.http.post<Order>('/api/orders', request);
  }

  getOrders(): Observable<OrderSummary[]> {
    return this.http.get<OrderSummary[]>('/api/orders');
  }

  getOrder(orderId: string): Observable<Order> {
    return this.http.get<Order>(`/api/orders/${orderId}`);
  }
}
