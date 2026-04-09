import { ChangeDetectorRef, Component, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { NavigationEnd, Router } from '@angular/router';
import { filter, timeout } from 'rxjs';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexFill,
  ApexGrid,
  ApexLegend,
  ApexPlotOptions,
  ApexTooltip,
  ApexXAxis,
  ApexYAxis
} from 'ng-apexcharts';
import { ReportService, VehicleProfitItem } from '../../../services/report';

type ChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  yaxis: ApexYAxis;
  dataLabels: ApexDataLabels;
  plotOptions: ApexPlotOptions;
  grid: ApexGrid;
  tooltip: ApexTooltip;
  legend: ApexLegend;
  fill: ApexFill;
  colors: string[];
};

@Component({
  selector: 'app-report-profit-vehicle',
  standalone: false,
  templateUrl: './report-profit-vehicle.html',
  styleUrl: './report-profit-vehicle.scss'
})
export class ReportProfitVehicleComponent implements OnInit {
  private readonly reportService = inject(ReportService);
  private readonly router = inject(Router);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly cdr = inject(ChangeDetectorRef);

  rows: VehicleProfitItem[] = [];
  reportYear = new Date().getFullYear();
  availableYears: number[] = [];
  loading = false;
  errorMessage = '';
  chartVisible = false;
  isBrowser = false;

  ngOnInit(): void {
    this.isBrowser = isPlatformBrowser(this.platformId);

    if (this.isBrowser) {
      queueMicrotask(() => this.load());

      this.router.events.pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd)
      ).subscribe(event => {
        if (event.urlAfterRedirects.startsWith('/Admin/reports/profit-vehicle')) {
          this.load();
        }
      });
    }
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';
    this.chartVisible = false;

    this.reportService.getVehicleProfit(this.reportYear).pipe(
      timeout(15000)
    ).subscribe({
      next: (data) => {
        const normalized = this.normalizeReportResponse(data);
        this.reportYear = normalized.year;
        this.availableYears = normalized.availableYears;
        this.rows = normalized.items;
        this.loading = false;
        this.refreshChart();
      },
      error: (err) => {
        this.errorMessage = err?.name === 'TimeoutError'
          ? 'A consulta do relatorio demorou demais. Tente novamente em alguns segundos.'
          : (err?.error?.message || 'Nao foi possivel carregar o relatorio.');
        this.loading = false;
        this.chartVisible = false;
        this.cdr.detectChanges();
      }
    });
  }

  refreshChart(): void {
    this.chartVisible = false;
    this.cdr.detectChanges();

    setTimeout(() => {
      this.chartVisible = true;
      this.cdr.detectChanges();
    }, 0);
  }

  private normalizeReportResponse(data: any): { year: number; availableYears: number[]; items: VehicleProfitItem[] } {
    const payload = data?.data ?? data;
    const year = Number(payload?.year ?? payload?.Year ?? new Date().getFullYear());
    const availableYears = Array.isArray(payload?.availableYears ?? payload?.AvailableYears)
      ? (payload?.availableYears ?? payload?.AvailableYears).map((item: any) => Number(item))
      : [year];
    const rawItems = payload?.items ?? payload?.Items ?? [];
    const items = Array.isArray(rawItems)
      ? rawItems.map((item: any) => ({
          vehicleId: item?.vehicleId ?? item?.VehicleId ?? '',
          licensePlate: item?.licensePlate ?? item?.LicensePlate ?? '',
          manufacturer: item?.manufacturer ?? item?.Manufacturer ?? '',
          isActive: Boolean(item?.isActive ?? item?.IsActive ?? false),
          revenue: Number(item?.revenue ?? item?.Revenue ?? 0),
          tripExpense: Number(item?.tripExpense ?? item?.TripExpense ?? 0),
          directExpense: Number(item?.directExpense ?? item?.DirectExpense ?? 0),
          commissionExpense: Number(item?.commissionExpense ?? item?.CommissionExpense ?? 0),
          cargoInsuranceExpense: Number(item?.cargoInsuranceExpense ?? item?.CargoInsuranceExpense ?? 0),
          totalExpense: Number(item?.totalExpense ?? item?.TotalExpense ?? 0),
          netProfit: Number(item?.netProfit ?? item?.NetProfit ?? 0)
        }))
      : [];

    return { year, availableYears, items };
  }

  get totalRevenue(): number {
    return this.rows.reduce((sum, item) => sum + Number(item.revenue || 0), 0);
  }

  get totalExpense(): number {
    return this.rows.reduce((sum, item) => sum + Number(item.totalExpense || 0), 0);
  }

  get totalProfit(): number {
    return this.rows.reduce((sum, item) => sum + Number(item.netProfit || 0), 0);
  }

  get profitableVehicles(): number {
    return this.rows.filter(item => Number(item.netProfit || 0) > 0).length;
  }

  get chartOptions(): Partial<ChartOptions> {
    const categories = this.rows.map(item => `${item.licensePlate} - ${item.manufacturer}`.trim());
    const data = this.rows.map(item => Number(item.netProfit || 0));

    return {
      series: [
        {
          name: 'Lucro Liquido',
          data
        }
      ],
      chart: {
        type: 'bar',
        height: Math.max(420, this.rows.length * 54),
        toolbar: {
          show: false
        },
        animations: {
          enabled: true,
          speed: 700
        },
        fontFamily: 'Segoe UI, sans-serif'
      },
      colors: data.map(value => value >= 0 ? '#16a34a' : '#dc2626'),
      plotOptions: {
        bar: {
          horizontal: true,
          borderRadius: 8,
          distributed: true,
          barHeight: '62%'
        }
      },
      dataLabels: {
        enabled: true,
        formatter: (value: number) => value.toLocaleString('pt-BR', {
          style: 'currency',
          currency: 'BRL',
          minimumFractionDigits: 2,
          maximumFractionDigits: 2
        }),
        style: {
          fontSize: '11px',
          fontWeight: '600'
        }
      },
      fill: {
        opacity: 1
      },
      grid: {
        borderColor: '#e2e8f0',
        strokeDashArray: 4
      },
      legend: {
        show: false
      },
      xaxis: {
        categories,
        labels: {
          formatter: (value: string) => Number(value).toLocaleString('pt-BR', {
            style: 'currency',
            currency: 'BRL',
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
          }),
          style: {
            colors: '#64748b',
            fontSize: '12px'
          }
        }
      },
      yaxis: {
        labels: {
          style: {
            colors: '#334155',
            fontSize: '12px'
          }
        }
      },
      tooltip: {
        theme: 'light',
        custom: ({ dataPointIndex }: { dataPointIndex: number }) => {
          const row = this.rows[dataPointIndex];
          if (!row) {
            return '';
          }

          const formatCurrency = (value: number) => value.toLocaleString('pt-BR', {
            style: 'currency',
            currency: 'BRL',
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
          });

          return `
            <div style="padding:12px 14px; min-width:240px;">
              <strong style="display:block; margin-bottom:8px; color:#0f172a;">${row.licensePlate} - ${row.manufacturer}</strong>
              <div style="display:grid; gap:4px; color:#334155; font-size:12px;">
                <span>Receita: ${formatCurrency(row.revenue)}</span>
                <span>Despesa da Viagem: ${formatCurrency(row.tripExpense)}</span>
                <span>Despesa do Veiculo: ${formatCurrency(row.directExpense)}</span>
                <span>Comissao: ${formatCurrency(row.commissionExpense)}</span>
                <span>Seguro da Carga: ${formatCurrency(row.cargoInsuranceExpense)}</span>
                <span>Total de Custos: ${formatCurrency(row.totalExpense)}</span>
                <span style="font-weight:700; color:${row.netProfit >= 0 ? '#166534' : '#b91c1c'};">Lucro Liquido: ${formatCurrency(row.netProfit)}</span>
              </div>
            </div>
          `;
        }
      }
    };
  }
}
