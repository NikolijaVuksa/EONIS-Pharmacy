import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CartItem } from '../models/cart-item';
import { Order, OrderItem } from '../models/order';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private apiUrl = 'https://localhost:7201/api/orders';
  constructor(private http: HttpClient) {}

  createOrder(items: CartItem[], customerEmail: string): Observable<Order> {
    const dto = {
      customerEmail,
      items: items.map(
        (ci) =>
          ({ productId: ci.product.id, quantity: ci.quantity } as OrderItem)
      ),
    };
    return this.http.post<Order>(this.apiUrl, dto);
  }

  placeOrder(orderId: number): Observable<Order> {
    return this.http.post<Order>(`${this.apiUrl}/${orderId}/place`, {});
  }

  payOrder(orderId: number): Observable<Order> {
    return this.http.post<Order>(`${this.apiUrl}/${orderId}/pay`, {});
  }
}
