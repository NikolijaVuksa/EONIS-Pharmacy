import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CartService } from '../services/cart.service';
import { OrderService } from '../services/order.service';
import { PaymentService } from '../services/payment.service';
import { loadStripe, Stripe, StripeCardElement } from '@stripe/stripe-js';
import { environment } from '../environments/environment';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css'],
})
export class CartComponent implements OnInit {
  items = this.cartService.getItems();
  total = 0;
  processing = false;
  stripe: Stripe | null = null;
  cardElement?: StripeCardElement;
  @ViewChild('cardInfo') cardInfo!: ElementRef;

  constructor(
    private cartService: CartService,
    private orderService: OrderService,
    private paymentService: PaymentService,
    private authService: AuthService
  ) {}

  async ngOnInit(): Promise<void> {
    this.cartService.items$.subscribe((items) => {
      this.items = items;
      this.total = this.cartService.getTotal();
    });

    this.stripe = await loadStripe(environment.stripePublishableKey);
    if (this.stripe) {
      const elements = this.stripe.elements();
      this.cardElement = elements.create('card');
      this.cardElement.mount(this.cardInfo.nativeElement);
    }
  }

  async updateQuantity(item: any, event: any): Promise<void> {
    const qty = Number(event.target.value);
    await this.cartService.updateQuantity(item.product.id, qty);
  }

  removeItem(item: any): void {
    this.cartService.removeItem(item.product.id);
  }

  async checkout(): Promise<void> {
    if (!this.items.length || this.processing) return;

    const email = this.authService.getEmail();
    if (!email) {
      alert('Morate biti prijavljeni da biste izvršili plaćanje.');
      return;
    }

    this.processing = true;

    try {
      const order = await this.orderService
        .createOrder(this.items, email)
        .toPromise();
      await this.orderService.placeOrder(order!.id).toPromise();

      const payment = await this.paymentService
        .createPayment(order!.id)
        .toPromise();

      const result = await this.stripe!.confirmCardPayment(
        payment!.clientSecret,
        {
          payment_method: {
            card: this.cardElement!,
            billing_details: { email: email },
          },
        }
      );

      if (result.error) {
        alert(result.error.message);
      } else {
        await this.paymentService.confirmPayment(order!.id).toPromise();
        alert('Plaćanje uspešno!');
        this.cartService.clearCart();
      }
    } catch (err) {
      console.error(err);
      alert('Došlo je do greške prilikom plaćanja.');
    } finally {
      this.processing = false;
    }
  }
}
