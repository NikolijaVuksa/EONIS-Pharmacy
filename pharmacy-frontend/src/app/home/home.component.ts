import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ProductService } from '../services/product.service';
import { CartService } from '../services/cart.service';
import { Product } from '../models/product';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent implements OnInit {
  products: Product[] = [];
  pagedProducts: Product[] = [];

  currentPage = 1;
  pageSize = 12;
  totalPages = 1;

  searchTerm: string = '';
  selectedCategory: string = '';
  rxFilter: string = '';

  filteredProducts: Product[] = [];
  categories: string[] = [];

  constructor(
    private productService: ProductService,
    private cartService: CartService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.productService.getProducts().subscribe({
      next: (data) => {
        this.products = data;

        this.categories = Array.from(
          new Set(this.products.map((p) => p.category).filter((c) => c))
        );

        this.filteredProducts = [...this.products];
        this.totalPages = Math.ceil(
          this.filteredProducts.length / this.pageSize
        );
        this.updatePagedProducts();
      },
      error: (err) => console.error(err),
    });
  }

  filterProducts(): void {
    const term = this.searchTerm.toLowerCase().trim();

    this.filteredProducts = this.products.filter((p) => {
      const matchesSearch =
        !term ||
        p.name.toLowerCase().includes(term) ||
        p.manufacturer.toLowerCase().includes(term) ||
        (p.category && p.category.toLowerCase().includes(term));

      const matchesCategory =
        !this.selectedCategory || p.category === this.selectedCategory;

      const matchesRx =
        this.rxFilter === ''
          ? true
          : this.rxFilter === 'true'
          ? p.rx === true
          : p.rx === false;

      return matchesSearch && matchesCategory && matchesRx;
    });

    this.totalPages = Math.ceil(this.filteredProducts.length / this.pageSize);
    this.currentPage = 1;
    this.updatePagedProducts();
  }

  updatePagedProducts(): void {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    this.pagedProducts = this.filteredProducts.slice(startIndex, endIndex);
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updatePagedProducts();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  goToProduct(id: number): void {
    this.router.navigate(['/product', id]);
  }

  addToCart(product: Product): void {
    this.cartService.addToCart(product).catch((err) => console.error(err));
  }

  onPageSizeChange(): void {
    this.currentPage = 1;
    this.totalPages = Math.ceil(this.filteredProducts.length / this.pageSize);
    this.updatePagedProducts();
  }
}
