import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TripService {
  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);
  private readonly apiUrl = `${environment.apiBaseUrl}/trips`; 
  private readonly expenseTypesUrl = `${environment.apiBaseUrl}/expense-types`;
  private readonly productsUrl = `${environment.apiBaseUrl}/products`;

  private getHeaders() {
    const token = isPlatformBrowser(this.platformId) ? localStorage.getItem('token') : null;
    return token
      ? new HttpHeaders({ Authorization: `Bearer ${token}` })
      : new HttpHeaders();
  }

  getTrips(): Observable<any> {
    return this.http.get<any>(this.apiUrl, { headers: this.getHeaders() });
  }

  getTripById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  openTrip(tripData: any): Observable<any> {
    const formData = new FormData();
    Object.keys(tripData).forEach(key => {
      if (tripData[key] !== null && tripData[key] !== undefined) {
        formData.append(key, this.serializeFormDataValue(tripData[key]));
      }
    });
    // FormData não deve ter Content-Type manual, o browser define o boundary automaticamente, 
    // mas o Authorization é obrigatório.
    return this.http.post<any>(`${this.apiUrl}/open`, formData, { headers: this.getHeaders() });
  }

  updateOpening(id: string, payload: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/opening`, payload, { headers: this.getHeaders() });
  }

  finishTrip(id: string, finishData: { unloadingDate: string, finishKm: number, unloadingLocation: string, unloadedWeightTons: number, freightValue: number, dieselKmPerLiter?: number | null, arlaKmPerLiter?: number | null }): Observable<any> {
    // Faltava o { headers } aqui
    return this.http.patch<any>(`${this.apiUrl}/${id}/finish`, finishData, { headers: this.getHeaders() });
  }

  reopenTrip(id: string): Observable<any> {
    return this.http.patch<any>(`${this.apiUrl}/${id}/reopen`, {}, { headers: this.getHeaders() });
  }

  cancelTrip(id: string): Observable<any> {
    return this.http.patch<any>(`${this.apiUrl}/${id}/cancel`, {}, { headers: this.getHeaders() });
  }

  // --- ÁREA DE DESPESAS ---

  addExpense(tripId: string, expenseData: any): Observable<any> {
    // Faltava o { headers } aqui
    return this.http.post<any>(`${this.apiUrl}/${tripId}/expenses`, expenseData, { headers: this.getHeaders() });
  }

  updateExpense(tripId: string, expenseId: string, expenseData: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${tripId}/expenses/${expenseId}`, expenseData, { headers: this.getHeaders() });
  }

  getExpenseTypes(): Observable<any> {
    // Faltava o { headers } aqui
    return this.http.get<any>(this.expenseTypesUrl, { headers: this.getHeaders() });
  }

  getProducts(): Observable<any> {
    return this.http.get<any>(this.productsUrl, { headers: this.getHeaders() });
  }

  private serializeFormDataValue(value: unknown): string | Blob {
    if (value instanceof Blob) {
      return value;
    }

    if (typeof value === 'number') {
      return value.toLocaleString('pt-BR', {
        useGrouping: false,
        maximumFractionDigits: 15
      });
    }

    return String(value);
  }
}
