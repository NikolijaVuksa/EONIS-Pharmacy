// src/app/services/product.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Product } from '../models/product';
import { StockBatch } from '../models/stock-batch';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = 'https://localhost:7201/api/products';

  constructor(private http: HttpClient) {}

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(this.apiUrl);
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  getAvailableStock(productId: number): Observable<number> {
    return this.http
      .get<StockBatch[]>(`${this.apiUrl}/${productId}/stockbatches`)
      .pipe(
        map((batches) => batches.reduce((sum, b) => sum + b.quantityOnHand, 0))
      );
  }
}
