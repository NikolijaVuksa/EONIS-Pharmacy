import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ImageUploadService {
  private baseUrl = 'https://localhost:7201/api/products';

  constructor(private http: HttpClient) {}

  upload(productId: number, file: File): Observable<{ imagePath: string }> {
    const formData = new FormData();
    formData.append('imageFile', file);
    return this.http.post<{ imagePath: string }>(
      `${this.baseUrl}/${productId}/image`,
      formData
    );
  }
}
