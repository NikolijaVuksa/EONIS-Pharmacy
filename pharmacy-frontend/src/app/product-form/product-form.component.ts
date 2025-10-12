import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ImageUploadService } from '../services/image-upload.service';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-product-form',
  templateUrl: './product-form.component.html',
})
export class ProductFormComponent implements OnInit {
  @Input() product: any = null; // ako se prosledi — radi se izmena
  @Output() formClose = new EventEmitter<void>();
  @Output() formSaved = new EventEmitter<void>();

  form!: FormGroup;
  selectedFile: File | null = null;
  previewUrl: string | ArrayBuffer | null = null;
  isEditMode = false;
  baseUrl = 'https://localhost:7201/api/products';

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private imageUpload: ImageUploadService
  ) {}

  ngOnInit() {
    this.isEditMode = !!this.product;

    this.form = this.fb.group({
      name: [
        this.product?.name || '',
        [Validators.required, Validators.maxLength(160)],
      ],
      rx: [this.product?.rx || false],
      basePrice: [
        this.product?.basePrice || 0,
        [Validators.required, Validators.min(0)],
      ],
      vatRate: [
        this.product?.vatRate || 10,
        [Validators.min(0), Validators.max(50)],
      ],
      manufacturer: [
        this.product?.manufacturer || '',
        Validators.maxLength(160),
      ],
      category: [this.product?.category || '', Validators.maxLength(120)],
      description: [this.product?.description || ''],
      imagePath: [this.product?.imagePath || null],
    });
  }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
    if (this.selectedFile) {
      const reader = new FileReader();
      reader.onload = () => (this.previewUrl = reader.result);
      reader.readAsDataURL(this.selectedFile);
    }
  }

  async uploadImage(productId: number): Promise<void> {
    if (!this.selectedFile) return;

    try {
      const res = await lastValueFrom(
        this.imageUpload.upload(productId, this.selectedFile)
      );
      console.log('Upload uspešan:', res.imagePath);
    } catch (err) {
      console.error('Greška pri uploadu slike:', err);
    }
  }

  async onSubmit() {
    if (this.form.invalid) return;

    const data = this.form.getRawValue();
    let productId = this.product?.id || null;

    try {
      if (this.isEditMode && productId) {
        // 🔹 Izmena postojećeg proizvoda
        await this.http.put(`${this.baseUrl}/${productId}`, data).toPromise();

        // Ako postoji nova slika — uploaduj
        if (this.selectedFile) await this.uploadImage(productId);
      } else {
        // 🔹 Kreiranje novog proizvoda
        const created: any = await this.http
          .post(this.baseUrl, data)
          .toPromise();
        productId = created.id;

        // Ako postoji slika — uploaduj (backend automatski čuva path)
        if (productId && this.selectedFile) await this.uploadImage(productId);
      }

      this.formSaved.emit();
      this.formClose.emit();
    } catch (error) {
      console.error('Greška prilikom čuvanja proizvoda:', error);
    }
  }

  cancel() {
    this.formClose.emit();
  }
}
