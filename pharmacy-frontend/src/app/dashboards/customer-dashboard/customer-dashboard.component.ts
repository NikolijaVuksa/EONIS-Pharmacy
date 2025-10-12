/*import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { DashboardService } from '../dashboards.component';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-customer-dashboard',
  templateUrl: './customer-dashboard.component.html',
  styleUrls: ['./customer-dashboard.component.css'],
})
export class CustomerDashboardComponent implements OnInit {
  userEmail: string | null = '';
  orders: any[] = [];

  constructor(
    private dashboardService: DashboardService,
    private authService: AuthService,
    private router: Router,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    const token = localStorage.getItem('authToken');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.userEmail =
          payload.FullName ||
          payload.email ||
          payload.sub ||
          payload.unique_name;
      } catch (err) {
        console.error('Greška pri čitanju tokena:', err);
      }
    }

    this.dashboardService.getMyOrders().subscribe({
      next: (orders) => {
        this.orders = orders;
      },
      error: (err) => {
        console.error('Greška pri učitavanju porudžbina:', err);
      },
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  getMyOrders(): Observable<any[]> {
    return this.http.get<any[]>('https://localhost:7201/api/orders/my-orders');
  }
*/
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { OrderService } from '../../services/order.service';

@Component({
  selector: 'app-customer-dashboard',
  templateUrl: './customer-dashboard.component.html',
  styleUrls: ['./customer-dashboard.component.css'],
})
export class CustomerDashboardComponent implements OnInit {
  userEmail: string | null = '';
  orders: any[] = [];
  loading = true;

  constructor(
    private orderService: OrderService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const token = localStorage.getItem('authToken');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.userEmail =
          payload.FullName ||
          payload.email ||
          payload.sub ||
          payload.unique_name;
      } catch (err) {
        console.error('Greška pri čitanju tokena:', err);
      }
    }

    this.orderService.getMyOrders().subscribe({
      next: (data) => {
        this.orders = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Greška pri učitavanju porudžbina:', err);
        this.loading = false;
      },
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }
}
