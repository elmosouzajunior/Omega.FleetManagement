import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface VehicleCostPerKmItem {
  vehicleId: string;
  licensePlate: string;
  manufacturer: string;
  totalKm: number;
  totalExpense: number;
  costPerKm: number;
}

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly apiUrl = `${environment.apiBaseUrl}/reports`;

  constructor(private readonly http: HttpClient) {}

  getVehicleCostPerKm(): Observable<VehicleCostPerKmItem[]> {
    return this.http.get<VehicleCostPerKmItem[]>(`${this.apiUrl}/cost-per-km`);
  }
}
