import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PaymentCreateResponse } from '../models/payment';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private apiUrl = 'https://localhost:7201/api/payments';
  constructor(private http: HttpClient) {}

  createPayment(orderId: number): Observable<PaymentCreateResponse> {
    return this.http.post<PaymentCreateResponse>(
      `${this.apiUrl}/create?orderId=${orderId}`,
      {}
    );
  }

  confirmPayment(orderId: number): Observable<void> {
    return this.http.post<void>(
      `${this.apiUrl}/confirm?orderId=${orderId}`,
      {}
    );
  }
}
