import { Component, OnInit, inject } from '@angular/core';
import { ReportService, VehicleCostPerKmItem } from '../../../services/report';

@Component({
  selector: 'app-report-cost-km',
  standalone: false,
  templateUrl: './report-cost-km.html',
  styleUrl: './report-cost-km.scss'
})
export class ReportCostKmComponent implements OnInit {
  private readonly reportService = inject(ReportService);

  rows: VehicleCostPerKmItem[] = [];
  loading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';

    this.reportService.getVehicleCostPerKm().subscribe({
      next: (data) => {
        this.rows = Array.isArray(data) ? data : [];
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Nao foi possivel carregar o relatorio.';
        this.loading = false;
      }
    });
  }
}
