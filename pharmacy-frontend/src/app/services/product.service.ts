import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
}

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private baseUrl = 'https://localhost:7201/api';

  constructor(private http: HttpClient) {}

  getProducts(): Observable<Product[]> {
    const url = `${this.baseUrl}/products`;
    return this.http.get<Product[]>(url);
  }

  getProduct(id: number): Observable<Product> {
    const url = `${this.baseUrl}/products/${id}`;
    return this.http.get<Product>(url);
  }
}
