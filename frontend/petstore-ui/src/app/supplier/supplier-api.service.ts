import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { InventoryItem } from '../admin/admin.models';

@Injectable({ providedIn: 'root' })
export class SupplierApiService {
  private readonly http = inject(HttpClient);

  getInventory(): Observable<InventoryItem[]> {
    return this.http.get<InventoryItem[]>('/api/supplier/inventory');
  }

  setInventory(itemId: string, quantity: number): Observable<InventoryItem> {
    return this.http.put<InventoryItem>(`/api/supplier/inventory/${itemId}`, { quantity });
  }

  runFulfillment(): Observable<{ processed: number }> {
    return this.http.post<{ processed: number }>('/api/supplier/fulfillment/run', null);
  }
}
