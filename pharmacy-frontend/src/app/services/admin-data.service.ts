import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface AdminOrderItem {
  id: number;
  productId: number;
  productName?: string;
  quantity: number;
  unitPrice: number;
  vatRate: number;
}

export interface AdminOrder {
  id: number;
  status: string;
  customerEmail?: string;
  customerName?: string;
  createdAt: string;
  items: AdminOrderItem[];
}

// ✅ Usklađeno sa backend modelom Product
export interface ProductCreateDto {
  name: string;
  basePrice: number;
  vatRate: number;
  rx: boolean;
  manufacturer: string;
  category: string;
  description?: string;
  imagePath?: string;
}

// DTO koji se vraća sa servera
export interface ProductReadDto extends ProductCreateDto {
  id: number;
  priceWithVat?: number;
}

@Injectable({ providedIn: 'root' })
export class AdminDataService {
  private api = 'https://localhost:7201/api';
  constructor(private http: HttpClient) {}

  getProducts() {
    return this.http.get<ProductReadDto[]>(`${this.api}/Products`);
  }

  createProduct(p: ProductCreateDto) {
    return this.http.post<ProductReadDto>(`${this.api}/Products`, p);
  }

  updateProduct(id: number, p: ProductCreateDto) {
    return this.http.put<ProductReadDto>(`${this.api}/Products/${id}`, p);
  }

  deleteProduct(id: number) {
    return this.http.delete(`${this.api}/Products/${id}`);
  }

  // ✅ Narudžbine
  getOrders() {
    return this.http.get<AdminOrder[]>(`${this.api}/Orders/all`);
  }

  updateOrderStatus(orderId: number, status: string) {
    return this.http.put(`${this.api}/Orders/${orderId}/status`, { status });
  }

  // ✅ Korisnici
  getUsers() {
    return this.http.get<any[]>(`${this.api}/admin/users`);
  }
}
