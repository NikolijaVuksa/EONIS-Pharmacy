import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../services/product.service';
import { CartService } from '../services/cart.service';
import { AuthService } from '../services/auth.service';
import { Product } from '../models/product';

@Component({
  selector: 'app-product-detail',
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css'],
})
export class ProductDetailComponent implements OnInit {
  product: Product | undefined;
  availableStock: number | undefined;
  selectedFile: File | null = null;
  errorMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private cartService: CartService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const productId = Number(this.route.snapshot.paramMap.get('id'));
    if (!productId) {
      return;
    }
    this.productService.getProduct(productId).subscribe({
      next: (data) => {
        this.product = data;
      },
      error: (err) => {
        console.error('Greška pri dobijanju proizvoda:', err);
      },
    });
    this.productService.getAvailableStock(productId).subscribe({
      next: (qty) => {
        this.availableStock = qty;
      },
      error: (err) => {
        console.error('Greška pri dobijanju zaliha:', err);
      },
    });
  }

  onFileSelected(event: Event): void {
    const fileInput = event.target as HTMLInputElement;
    if (fileInput.files && fileInput.files.length > 0) {
      this.selectedFile = fileInput.files[0];
    }
  }

  uploadImage(): void {
    if (!this.product || !this.selectedFile) return;
    this.productService
      .uploadProductImage(this.product.id, this.selectedFile)
      .subscribe({
        next: (res: any) => {
          alert('Slika uspešno otpremljena.');
          if (this.product) {
            this.product.imagePath = res.imagePath;
          }
        },
        error: (err) => {
          console.error('Greška pri otpremanju slike:', err);
          alert('Greška pri otpremanju slike.');
        },
      });
  }

  addToCart(): void {
    if (!this.product) return;

    this.errorMessage = '';

    this.cartService
      .addToCart(this.product)
      .then(() => {
        this.errorMessage = '';
      })
      .catch((err) => {
        if (err.message) {
          this.errorMessage = err.message;
        } else {
          console.error(err);
        }
      });
  }
}
