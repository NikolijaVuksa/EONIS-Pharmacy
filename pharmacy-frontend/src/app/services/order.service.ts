// src/app/services/order.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CartItem } from '../models/cart-item';
import { Order, OrderItem } from '../models/order';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private apiUrl = 'https://localhost:7201/api/orders';
  constructor(private http: HttpClient) {}

  /**
   * Kreira novu porudžbinu u statusu Draft. Prosleđuje email kupca i listu stavki (productId, quantity).
   */
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

  /**
   * Finalizuje narudžbinu (rezerviše količine u magacinu). Nakon ovoga se ne može menjati.
   */
  placeOrder(orderId: number): Observable<Order> {
    return this.http.post<Order>(`${this.apiUrl}/${orderId}/place`, {});
  }

  /**
   * Obeležava da je narudžbina plaćena (bez Stripe webhook‑a). Opcionalno se može koristiti nakon uspešnog plaćanja.
   */
  payOrder(orderId: number): Observable<Order> {
    return this.http.post<Order>(`${this.apiUrl}/${orderId}/pay`, {});
  }
}
