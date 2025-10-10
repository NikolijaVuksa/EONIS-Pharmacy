import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';
import { CartService } from './services/cart.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  isLoggedIn$ = this.authService.isLoggedIn$;
  cartItemCount = 0;
  private cartSub: Subscription;

  constructor(
    private router: Router,
    private authService: AuthService,
    private cartService: CartService
  ) {
    this.cartSub = this.cartService.items$.subscribe(
      (items) =>
        (this.cartItemCount = items.reduce((sum, it) => sum + it.quantity, 0))
    );
  }

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }
}
