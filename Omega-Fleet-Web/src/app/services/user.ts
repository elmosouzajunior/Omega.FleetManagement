import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, shareReplay, throwError, tap } from 'rxjs';
import { User } from '../shared/models/user';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly apiUrl = environment.apiBaseUrl;
  private usersCache$: Observable<User[]> | null = null;

  constructor(private readonly http: HttpClient) {}

  getAllUsersCached(forceRefresh = false): Observable<User[]> {
    if (!this.usersCache$ || forceRefresh) {
      this.usersCache$ = this.http.get<User[]>(`${this.apiUrl}/companyAdmins`).pipe(
        shareReplay(1),
        catchError((err) => {
          this.usersCache$ = null;
          return throwError(() => err);
        })
      );
    }

    return this.usersCache$;
  }

  createUser(companyAdminData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/companyAdmins`, companyAdminData).pipe(
      tap(() => this.invalidateUsersCache())
    );
  }

  updateUser(id: string, payload: { adminFullName: string; adminEmail: string }): Observable<any> {
    return this.http.put(`${this.apiUrl}/companyAdmins/${id}`, payload).pipe(
      tap(() => this.invalidateUsersCache())
    );
  }

  deactivateUser(id: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/companyAdmins/${id}/deactivate`, {}).pipe(
      tap(() => this.invalidateUsersCache())
    );
  }

  reactivateUser(id: string): Observable<any> {
    return this.http.patch(`${this.apiUrl}/companyAdmins/${id}/reactivate`, {}).pipe(
      tap(() => this.invalidateUsersCache())
    );
  }

  invalidateUsersCache(): void {
    this.usersCache$ = null;
  }
}
