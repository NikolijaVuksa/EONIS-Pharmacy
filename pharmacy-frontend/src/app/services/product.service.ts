import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, throwError } from 'rxjs';
import { Product } from '../models/product';
import { StockBatch } from '../models/stock-batch';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = '/api/Products';

  constructor(private http: HttpClient) {}

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(this.apiUrl);
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  getAvailableStock(productId: number): Observable<number> {
    return this.getProductById(productId).pipe(
      map((product) => {
        if (product.totalStock == null || product.totalStock === undefined) {
          throw new Error('totalStock property missing on Product');
        }
        return product.totalStock;
      }),
      catchError((error) => {
        console.error('Error fetching stock for product', error);
        return throwError(() => error);
      })
    );
  }

  uploadProductImage(productId: number, file: File): Observable<any> {
    const uploadUrl = `${this.apiUrl}/${productId}/image`;
    const formData = new FormData();
    formData.append('imageFile', file);
    return this.http.post(uploadUrl, formData);
  }
}
