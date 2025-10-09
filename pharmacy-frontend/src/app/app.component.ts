import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  isLoggedIn$ = this.authService.isLoggedIn$;

  constructor(private router: Router, private authService: AuthService) {}

  onLogout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }
}
