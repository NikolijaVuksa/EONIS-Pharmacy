// src/app/services/payment.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PaymentCreateResponse } from '../models/payment';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private apiUrl = 'https://localhost:7201/api/payments';
  constructor(private http: HttpClient) {}

  /**
   * Kreira Stripe PaymentIntent i vraća clientSecret. Backend očekuje parametar orderId.
   */
  createPayment(orderId: number): Observable<PaymentCreateResponse> {
    return this.http.post<PaymentCreateResponse>(
      `${this.apiUrl}/create?orderId=${orderId}`,
      {}
    );
  }

  /**
   * Potvrđuje backend‑u da je plaćanje uspešno (koristi se ako nemate webhook). Prosledite isti orderId.
   */
  confirmPayment(orderId: number): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/confirm?orderId=${orderId}`,
      {}
    );
  }
}
