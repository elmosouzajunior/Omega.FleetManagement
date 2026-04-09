import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface VehicleCostPerKmItem {
  vehicleId: string;
  licensePlate: string;
  manufacturer: string;
  isActive: boolean;
  months: VehicleCostPerKmMonthlyMetric[];
  annualAverageCostPerKm: number;
}

export interface VehicleCostPerKmMonthlyMetric {
  month: number;
  totalKm: number;
  totalExpense: number;
  costPerKm: number;
  expenseTypes: VehicleCostPerKmExpenseTypeMetric[];
}

export interface VehicleCostPerKmExpenseTypeMetric {
  expenseTypeName: string;
  totalExpense: number;
  costPerKm: number;
}

export interface VehicleCostPerKmReportResponse {
  year: number;
  availableYears: number[];
  items: VehicleCostPerKmItem[];
}

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly apiUrl = `${environment.apiBaseUrl}/reports`;

  constructor(private readonly http: HttpClient) {}

  getVehicleCostPerKm(year?: number): Observable<VehicleCostPerKmReportResponse> {
    const query = typeof year === 'number' ? `?year=${year}` : '';
    return this.http.get<VehicleCostPerKmReportResponse>(`${this.apiUrl}/cost-per-km${query}`);
  }
}
