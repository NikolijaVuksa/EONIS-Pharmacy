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
  allOrders: AdminOrder[] = []; // sve porudžbine sa backenda
  orders: AdminOrder[] = []; // samo trenutna stranica
  statuses = ['Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'];
  loading = false;
  selectedOrder: AdminOrder | null = null;

  currentPage = 1;
  pageSize = 20;
  totalPages = 1;

  constructor(private admin: AdminDataService) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.admin.getOrders().subscribe({
      next: (data) => {
        this.allOrders = data;
        this.totalPages = Math.ceil(this.allOrders.length / this.pageSize);
        this.updatePagedOrders();
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  updatePagedOrders() {
    const start = (this.currentPage - 1) * this.pageSize;
    this.orders = this.allOrders.slice(start, start + this.pageSize);
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.updatePagedOrders();
    }
  }

  prevPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.updatePagedOrders();
    }
  }

  showDetails(order: AdminOrder) {
    this.selectedOrder = order;
  }

  saveStatus(o: AdminOrder) {
    this.admin.updateOrderStatus(o.id, o.status).subscribe({
      next: () => {
        // opcionalno toast ili reload
      },
      error: () => {
        // prikazi gresku
      },
    });
  }

  getTotal(o: any): number {
    if (!o.items) return 0;
    return o.items
      .map((it: any) => it.unitPrice * it.quantity * (1 + it.vatRate / 100))
      .reduce((a: number, b: number) => a + b, 0);
  }

  statusClass(s: string): string {
    switch (s) {
      case 'Paid':
        return 'badge bg-success';
      case 'Payment Failed':
        return 'badge bg-danger';
      case 'Refunded':
        return 'badge bg-secondary';
      case 'Payment Pending':
        return 'badge bg-warning text-dark';
      default:
        return 'badge bg-light text-dark';
    }
  }

  toggleDetails(order: AdminOrder) {
    this.selectedOrder = this.selectedOrder === order ? null : order;
  }

  closeDetails() {
    this.selectedOrder = null;
  }
}
