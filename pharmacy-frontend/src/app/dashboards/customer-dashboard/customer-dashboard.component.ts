import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { OrderService } from '../../services/order.service';

export interface CustomerProfile {
  address?: string;
  city?: string;
  postalCode?: string;
  dateOfBirth?: Date;
  insuranceNumber?: string;
}

@Component({
  selector: 'app-customer-dashboard',
  templateUrl: './customer-dashboard.component.html',
  styleUrls: ['./customer-dashboard.component.css'],
})
export class CustomerDashboardComponent implements OnInit {
  userEmail: string | null = '';
  orders: any[] = [];
  profile: CustomerProfile | null = null;
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

      this.authService.getMyProfile().subscribe({
        next: (data) => (this.profile = data),
        error: (err) => console.error('Greška pri učitavanju profila:', err),
      });
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
