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

export interface ProductCreateDto {
  name: string;
  basePrice: number;
  vatRate: number;
  rx: boolean;
  manufacturer: string;
  category: string;
  description?: string;
  imagePath?: string;
  totalStock: number; // ✅ novo polje
}

export interface ProductReadDto extends ProductCreateDto {
  id: number;
  priceWithVat?: number;
}

export interface UserReadDto {
  id: string;
  email: string;
  userName: string;
  fullName: string;
  roles: string[];
}

export interface UserCreateDto {
  email: string;
  fullName: string;
  password: string;
  role: string; // "Admin" ili "Customer"
}

export interface OrderItemReadDto {
  id: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  vatRate: number;
}

export interface AdminOrderReadDto {
  id: number;
  status: string;
  customerEmail?: string;
  customerName?: string;
  createdAt: string;
  items: OrderItemReadDto[];
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

  getOrders() {
    return this.http.get<AdminOrderReadDto[]>(`${this.api}/Orders/all`);
  }

  updateOrderStatus(orderId: number, status: string) {
    return this.http.put(`${this.api}/Orders/${orderId}/status`, { status });
  }

  getUsers() {
    return this.http.get<UserReadDto[]>(`${this.api}/admin/users`);
  }

  createUser(user: UserCreateDto) {
    return this.http.post(`${this.api}/admin/users`, user);
  }

  updateUserRole(id: string, role: string) {
    return this.http.put(`${this.api}/admin/users/${id}/role`, { role });
  }

  deleteUser(id: string) {
    return this.http.delete(`${this.api}/admin/users/${id}`);
  }
}
