import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, forkJoin } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private baseUrl = 'https://localhost:7201/api';

  constructor(private http: HttpClient) {}

  getCustomerDashboard(): Observable<any> {
    const token = localStorage.getItem('authToken');
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return forkJoin({
      orders: this.http.get(`${this.baseUrl}/orders/my-orders`, { headers }),
      therapies: this.http.get(`${this.baseUrl}/therapies`, { headers }),
      reservations: this.http.get(`${this.baseUrl}/reservations`, { headers }),
    });
  }
}
