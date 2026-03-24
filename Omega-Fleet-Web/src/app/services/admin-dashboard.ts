import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

// Interface para tipar o que o dashboard espera receber
export interface AdminDashboardData {
  stats: {
    activeDrivers: number;
    activeVehicles: number;
    openTrips: number;
  };
  expenses: any[]; // Você pode tipar isso melhor depois conforme seu DTO
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  // Este método vai buscar todos os dados da "Land Page" em uma única chamada
  getAdminStats(): Observable<AdminDashboardData> {
    return this.http.get<AdminDashboardData>(`${this.apiUrl}/dashboard/admin-summary`);
  }
}
