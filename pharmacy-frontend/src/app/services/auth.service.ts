import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private authUrl = 'https://localhost:7201/api/auth';
  private _isLoggedIn$ = new BehaviorSubject<boolean>(this.hasToken());
  isLoggedIn$ = this._isLoggedIn$.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  // 🔹 Proverava da li token postoji u localStorage
  private hasToken(): boolean {
    return !!localStorage.getItem('authToken');
  }

  // 🔹 Login – čuva token i vodi korisnika na odgovarajući dashboard
  login(email: string, password: string): Observable<any> {
    return this.http
      .post<any>(`${this.authUrl}/login`, { email, password })
      .pipe(
        tap((res) => {
          if (res?.token) {
            localStorage.setItem('authToken', res.token);
            this._isLoggedIn$.next(true);

            const role = this.getUserRole();
            if (role === 'Admin') {
              this.router.navigate(['/admin']);
            } else {
              this.router.navigate(['/dashboard']);
            }
          }
        })
      );
  }

  logout(): void {
    localStorage.removeItem('authToken');
    this._isLoggedIn$.next(false);
    this.router.navigate(['/']);
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  getDisplayName(): string | null {
    const t = this.getToken();
    if (!t) return null;
    try {
      const p = JSON.parse(atob(t.split('.')[1]));
      return p.FullName || p.email || p.name || 'Korisnik';
    } catch {
      return null;
    }
  }

  getEmail(): string | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return (
        payload.email ||
        payload.Email ||
        payload[
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'
        ] ||
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
        null
      );
    } catch {
      return null;
    }
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const now = Math.floor(Date.now() / 1000);
      return payload.exp && payload.exp > now;
    } catch {
      return false;
    }
  }

  getUserRole(): string | null {
    const token = localStorage.getItem('authToken');
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const roleClaim =
        payload['role'] ||
        payload['roles'] ||
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

      if (Array.isArray(roleClaim)) {
        return roleClaim[0];
      }
      return roleClaim || null;
    } catch {
      return null;
    }
  }

  isAdmin(): boolean {
    const role = this.getUserRole();
    return this.isLoggedIn() && role === 'Admin';
  }

  isCustomer(): boolean {
    const role = this.getUserRole();
    return this.isLoggedIn() && role === 'Customer';
  }
}
