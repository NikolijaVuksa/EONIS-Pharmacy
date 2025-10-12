import { Component, OnInit } from '@angular/core';
import {
  AdminDataService,
  AdminOrder,
} from '../../services/admin-data.service';

@Component({
  selector: 'app-admin-orders',
  templateUrl: './admin-orders.component.html',
})
export class AdminOrdersComponent implements OnInit {
  orders: AdminOrder[] = [];
  statuses = ['Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'];
  loading = false;

  constructor(private admin: AdminDataService) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.admin.getOrders().subscribe({
      next: (data) => {
        this.orders = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  saveStatus(o: AdminOrder) {
    this.admin.updateOrderStatus(o.id, o.status).subscribe({
      next: () => {
        /* opcionalno toast */
      },
      error: () => {
        /* prikazi gresku */
      },
    });
  }
}
