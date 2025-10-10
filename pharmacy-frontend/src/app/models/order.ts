// src/app/models/order.ts
import { CartItem } from './cart-item';

export interface OrderItem {
  productId: number;
  quantity: number;
}

export interface Order {
  id: number;
  status: string;
  customerEmail: string;
  createdAt: string;
  items: OrderItem[];
  reservationId?: number;
}
