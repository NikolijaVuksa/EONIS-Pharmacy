import { Component, OnInit } from '@angular/core';
import {
  AdminDataService,
  AdminOrder,
} from '../../services/admin-data.service';

@Component({
  selector: 'app-admin-orders',
  templateUrl: './admin-orders.component.html',
  styleUrls: ['./admin-orders.component.css'],
})
export class AdminOrdersComponent implements OnInit {
  allOrders: AdminOrder[] = []; // sve porudžbine sa backenda
  pagedOrders: AdminOrder[] = []; // prikazane na trenutnoj stranici
  loading = false;
  selectedOrder: AdminOrder | null = null;

  currentPage = 1;
  pageSize = 6; // 6 po strani
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
    this.pagedOrders = this.allOrders.slice(start, start + this.pageSize);
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

  toggleDetails(order: AdminOrder) {
    this.selectedOrder = this.selectedOrder === order ? null : order;
  }

  closeDetails() {
    this.selectedOrder = null;
  }

  saveStatus(order: AdminOrder) {
    this.admin.updateOrderStatus(order.id, order.status).subscribe({
      next: () => {
        console.log(`Status porudžbine #${order.id} ažuriran`);
      },
      error: () => {
        console.error('Greška pri ažuriranju statusa');
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
        return 'bg-success';
      case 'Payment Failed':
        return 'bg-danger';
      case 'Refunded':
        return 'bg-secondary';
      case 'Payment Pending':
        return 'bg-warning text-dark';
      default:
        return 'bg-light text-dark';
    }
  }

  cardBorderClass(s: string): string {
    switch (s) {
      case 'Paid':
        return 'border-success';
      case 'Payment Failed':
        return 'border-danger';
      case 'Refunded':
        return 'border-secondary';
      case 'Payment Pending':
        return 'border-warning';
      default:
        return 'border-light';
    }
  }
}
