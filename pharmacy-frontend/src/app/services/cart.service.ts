// src/app/services/cart.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { Product } from '../models/product';
import { CartItem } from '../models/cart-item';
import { ProductService } from './product.service';

@Injectable({ providedIn: 'root' })
export class CartService {
  private items: CartItem[] = [];
  private itemsSubject = new BehaviorSubject<CartItem[]>(this.items);
  /** Observable na koji se mogu pretplatiti komponente za osvežavanje liste. */
  items$ = this.itemsSubject.asObservable();

  constructor(private productService: ProductService) {}

  /** Vraća trenutne stavke (sinhrono). */
  getItems(): CartItem[] {
    return this.items;
  }

  /** Briše sve stavke iz korpe. */
  clearCart(): void {
    this.items = [];
    this.itemsSubject.next(this.items);
  }

  /** Dodaje proizvod u korpu. Ako proizvod već postoji, povećava količinu. Prvo se proverava zaliha. */
  async addToCart(product: Product, quantity: number = 1): Promise<void> {
    const availableStock = await firstValueFrom(
      this.productService.getAvailableStock(product.id)
    );
    const existingIndex = this.items.findIndex(
      (ci) => ci.product.id === product.id
    );
    const existingQty =
      existingIndex >= 0 ? this.items[existingIndex].quantity : 0;
    if (existingQty + quantity > availableStock) {
      alert('Nema dovoljno proizvoda na stanju. Dostupno: ' + availableStock);
      return;
    }
    if (existingIndex >= 0) {
      this.items[existingIndex].quantity += quantity;
    } else {
      this.items.push({ product, quantity });
    }
    this.itemsSubject.next(this.items);
  }

  /** Uklanja proizvod iz korpe. */
  removeItem(productId: number): void {
    this.items = this.items.filter((ci) => ci.product.id !== productId);
    this.itemsSubject.next(this.items);
  }

  /** Postavlja količinu stavke. Ako je količina 0 ili manja, stavka se briše. */
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
    if (quantity > availableStock) {
      alert('Nema dovoljno proizvoda na stanju. Dostupno: ' + availableStock);
      return;
    }
    this.items[index].quantity = quantity;
    this.itemsSubject.next(this.items);
  }

  /** Izračunava ukupan iznos (cena sa PDV‑om * količina) u korpi. */
  getTotal(): number {
    return this.items.reduce(
      (sum, item) => sum + item.product.priceWithVat * item.quantity,
      0
    );
  }
}
