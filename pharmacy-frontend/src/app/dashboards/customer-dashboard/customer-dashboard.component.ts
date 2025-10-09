import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { DashboardService } from '../dashboards.component';

@Component({
  selector: 'app-customer-dashboard',
  templateUrl: './customer-dashboard.component.html',
  styleUrls: ['./customer-dashboard.component.css'],
})
export class CustomerDashboardComponent implements OnInit {
  userEmail: string | null = '';
  orders: any[] = [];
  therapies: any[] = [];
  reservations: any[] = [];

  constructor(
    private dashboardService: DashboardService,
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

    this.dashboardService.getCustomerDashboard().subscribe({
      next: (data) => {
        this.orders = data.orders;
        this.therapies = data.therapies;
        this.reservations = data.reservations;
      },
      error: (err) => {
        console.error('Greška pri učitavanju podataka:', err);
      },
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }
}
