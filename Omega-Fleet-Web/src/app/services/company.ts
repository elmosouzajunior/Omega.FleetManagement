import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay } from 'rxjs';
import { Company } from '../shared/models/company';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CompanyService {
  private readonly apiUrl = environment.apiBaseUrl;
  private companiesCache$: Observable<Company[]> | null = null;

  constructor(private readonly http: HttpClient) { }

  getCompanies(): Observable<Company[]> {
    return this.http.get<Company[]>(`${this.apiUrl}/companies`);
  }

  getCompaniesCached(forceRefresh = false): Observable<Company[]> {
    if (!this.companiesCache$ || forceRefresh) {
      this.companiesCache$ = this.getCompanies().pipe(shareReplay(1));
    }

    return this.companiesCache$;
  }

  createCompany(companyData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/companies`, companyData);
  }

  updateCompany(id: string, companyData: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/companies/${id}`, companyData);
  }
}
