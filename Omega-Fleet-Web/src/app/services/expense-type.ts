import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ExpenseTypeService {
  private readonly apiUrl = `${environment.apiBaseUrl}/expense-types`;

  constructor(private http: HttpClient) {}

  getExpenseTypes(companyId?: string, includeInactive = false): Observable<any> {
    const query = new URLSearchParams();
    if (companyId) query.set('companyId', companyId);
    if (includeInactive) query.set('includeInactive', 'true');
    const url = query.toString() ? `${this.apiUrl}?${query.toString()}` : this.apiUrl;
    return this.http.get<any>(url);
  }

  createExpenseType(payload: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, payload);
  }

  updateExpenseType(id: string, payload: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, payload);
  }

  updateStatus(id: string, isActive: boolean): Observable<any> {
    return this.http.patch<any>(`${this.apiUrl}/${id}/status`, { isActive });
  }
}
