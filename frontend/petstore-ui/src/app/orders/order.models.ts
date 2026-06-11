import { ContactInfo } from '../identity/identity.models';

export interface OrderLine {
  itemId: string;
  name: string;
  unitPrice: number;
  currency: string;
  quantity: number;
  subtotal: number;
}

export interface Order {
  orderId: string;
  placedAt: string;
  status: string;
  total: number;
  currency: string;
  shippingContact: ContactInfo;
  billingContact: ContactInfo;
  lines: OrderLine[];
}

export interface OrderSummary {
  orderId: string;
  placedAt: string;
  total: number;
  currency: string;
  status: string;
}

export interface PlaceOrderRequest {
  shippingContact: ContactInfo;
  billingContact?: ContactInfo | null;
}
