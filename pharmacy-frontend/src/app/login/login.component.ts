import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent {
  email: string = '';
  password: string = '';
  errorMessage: string = '';

  constructor(private authService: AuthService, private router: Router) {}

  onLogin(): void {
    this.errorMessage = '';

    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        const role = this.authService.getUserRole();
        if (role === 'Admin') {
          this.router.navigate(['/admin']);
        } else if (role === 'Customer') {
          this.router.navigate(['/dashboard']);
        } else {
          this.router.navigate(['/']);
        }
      },
      error: () =>
        (this.errorMessage = 'Neispravni kredencijali. Pokušajte ponovo.'),
    });
  }
}
