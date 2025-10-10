import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private authUrl = 'https://localhost:7201/api/auth';

  private _isLoggedIn$ = new BehaviorSubject<boolean>(this.hasToken());
  isLoggedIn$ = this._isLoggedIn$.asObservable();

  constructor(private http: HttpClient) {}

  private hasToken(): boolean {
    return !!localStorage.getItem('authToken');
  }

  login(email: string, password: string): Observable<any> {
    return this.http
      .post<any>(`${this.authUrl}/login`, { email, password })
      .pipe(
        tap((res) => {
          if (res?.token) {
            localStorage.setItem('authToken', res.token);
            this._isLoggedIn$.next(true);
          }
        })
      );
  }

  logout(): void {
    localStorage.removeItem('authToken');
    this._isLoggedIn$.next(false);
  }

  getToken(): string | null {
    return localStorage.getItem('authToken');
  }

  getDisplayName(): string | null {
    const t = this.getToken();
    if (!t) return null;
    try {
      const p = JSON.parse(atob(t.split('.')[1]));
      return p.FullName || p.email || 'Korisnik';
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
}
