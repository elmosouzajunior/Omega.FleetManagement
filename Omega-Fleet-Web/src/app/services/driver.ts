import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Driver } from '../shared/models/driver';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class DriverService {
  private apiUrl = `${environment.apiBaseUrl}/drivers`;

  constructor(private http: HttpClient) { }

  createDriver(driver: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/create`, driver);
  }

  getDrivers(): Observable<Driver[]> {
    return this.http.get<Driver[]>(this.apiUrl);
  }

  update(id: string, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/update/${id}`, data);
  }
}

