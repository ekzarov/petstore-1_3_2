export interface CartLine {
  itemId: string;
  name: string;
  unitPrice: number;
  currency: string;
  quantity: number;
  subtotal: number;
  unavailable: boolean;
}

export interface Cart {
  lines: CartLine[];
  itemCount: number;
  total: number;
  currency: string;
}
