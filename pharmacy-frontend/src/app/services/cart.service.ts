import { Injectable } from '@angular/core';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { Product } from '../models/product';
import { CartItem } from '../models/cart-item';
import { ProductService } from './product.service';

@Injectable({ providedIn: 'root' })
export class CartService {
  private items: CartItem[] = [];
  private itemsSubject = new BehaviorSubject<CartItem[]>(this.items);
  items$ = this.itemsSubject.asObservable();

  constructor(private productService: ProductService) {}

  getItems(): CartItem[] {
    return this.items;
  }

  clearCart(): void {
    this.items = [];
    this.itemsSubject.next(this.items);
  }

  async addToCart(product: Product, quantity: number = 1): Promise<void> {
    if (!product) return;

    const availableStock = await firstValueFrom(
      this.productService.getAvailableStock(product.id)
    );
    const existingIndex = this.items.findIndex(
      (ci) => ci.product.id === product.id
    );
    const existingQty =
      existingIndex >= 0 ? this.items[existingIndex].quantity : 0;
    if (existingQty + quantity > availableStock) {
      throw new Error('Nema dovoljno proizvoda na stanju.');
    }

    if (existingIndex >= 0) {
      this.items[existingIndex].quantity += quantity;
    } else {
      this.items.push({ product, quantity });
    }
    this.itemsSubject.next(this.items);
  }

  removeItem(productId: number): void {
    this.items = this.items.filter((ci) => ci.product.id !== productId);
    this.itemsSubject.next(this.items);
  }

  async updateQuantity(productId: number, quantity: number): Promise<void> {
    const index = this.items.findIndex((ci) => ci.product.id === productId);
    if (index < 0) return;
    if (quantity <= 0) {
      this.removeItem(productId);
      return;
    }
    const availableStock = await firstValueFrom(
      this.productService.getAvailableStock(productId)
    );
    const existingIndex = this.items.findIndex(
      (ci) => ci.product.id === productId
    );
    const existingQty =
      existingIndex >= 0 ? this.items[existingIndex].quantity : 0;
    if (existingQty + quantity > availableStock) {
      throw new Error('Nema dovoljno proizvoda na stanju.');
    }
    this.items[index].quantity = quantity;
    this.itemsSubject.next(this.items);
  }

  getTotal(): number {
    return this.items.reduce(
      (sum, item) => sum + item.product.priceWithVat * item.quantity,
      0
    );
  }
}
