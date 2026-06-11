import { ContactInfo } from '../identity/identity.models';
import { OrderLine } from '../orders/order.models';

export interface AdminOrderSummary {
  orderId: string;
  placedAt: string;
  userId: string;
  total: number;
  currency: string;
  status: string;
}

export interface AdminOrderDetail {
  orderId: string;
  placedAt: string;
  userId: string;
  status: string;
  total: number;
  currency: string;
  shippingContact: ContactInfo;
  billingContact: ContactInfo;
  lines: OrderLine[];
}

export interface OrderTransition {
  fromStatus: string;
  toStatus: string;
  actor: string;
  occurredAt: string;
}

export interface InventoryItem {
  itemId: string;
  quantityOnHand: number;
}

export const ORDER_STATUSES = [
  'PENDING',
  'APPROVED',
  'DENIED',
  'SHIPPED_PART',
  'SHIPPED',
  'COMPLETED'
] as const;
