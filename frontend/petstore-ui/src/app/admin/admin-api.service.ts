import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminOrderDetail, AdminOrderSummary, OrderTransition } from './admin.models';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly http = inject(HttpClient);

  getOrders(status?: string): Observable<AdminOrderSummary[]> {
    const query = status ? `?status=${status}` : '';
    return this.http.get<AdminOrderSummary[]>(`/api/admin/orders${query}`);
  }

  getOrder(orderId: string): Observable<AdminOrderDetail> {
    return this.http.get<AdminOrderDetail>(`/api/admin/orders/${orderId}`);
  }

  approve(orderId: string): Observable<void> {
    return this.http.post<void>(`/api/admin/orders/${orderId}/approve`, null);
  }

  deny(orderId: string): Observable<void> {
    return this.http.post<void>(`/api/admin/orders/${orderId}/deny`, null);
  }

  getTransitions(orderId: string): Observable<OrderTransition[]> {
    return this.http.get<OrderTransition[]>(`/api/admin/orders/${orderId}/transitions`);
  }
}
