// vehicle.service.ts
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Vehicle } from '../shared/models/vehicle';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class VehicleService {
  private apiUrl = `${environment.apiBaseUrl}/vehicles`;

  constructor(private http: HttpClient) { }

  create(vehicle: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/create`, vehicle);
  }

  getVehicles(): Observable<Vehicle[]> {
    return this.http.get<Vehicle[]>(this.apiUrl);
  }

  assignDriver(vehicleId: string, driverId: string | null): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${vehicleId}/assign-driver`, { driverId });
  }

  update(id: string, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/update/${id}`, data);
  }

  addExpense(vehicleId: string, expenseData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/${vehicleId}/expenses`, expenseData);
  }
}
