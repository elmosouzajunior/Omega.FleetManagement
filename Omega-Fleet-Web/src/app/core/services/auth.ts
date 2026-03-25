// src/app/services/auth.service.ts
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiBaseUrl = environment.apiBaseUrl?.trim();
  private readonly apiUrl = `${this.apiBaseUrl}/auth/login`;

  constructor(private http: HttpClient) {}

  login(credentials: any): Observable<any> {
    if (!this.apiBaseUrl || this.apiBaseUrl.includes('__API_BASE_URL__')) {
      return throwError(() => new Error('API_BASE_URL_NOT_CONFIGURED'));
    }

    return this.http.post(this.apiUrl, credentials).pipe(
      tap((response: any) => {
        if (response.token) {
          localStorage.setItem('token', response.token);
          localStorage.setItem('user', JSON.stringify(response.user));
        }
      }),
      catchError((error) => throwError(() => error))
    );
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token'); // Retorna true se houver token
  }
}
