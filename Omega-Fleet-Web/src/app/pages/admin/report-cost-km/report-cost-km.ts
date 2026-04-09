import { ChangeDetectorRef, Component, OnInit, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ReportService, VehicleCostPerKmItem, VehicleCostPerKmMonthlyMetric, VehicleCostPerKmExpenseTypeMetric } from '../../../services/report';
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
  ApexStroke,
  ApexTooltip,
  ApexXAxis,
  ApexYAxis
} from 'ng-apexcharts';

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
  stroke: ApexStroke;
  fill: ApexFill;
  colors: string[];
};

@Component({
  selector: 'app-report-cost-km',
  standalone: false,
  templateUrl: './report-cost-km.html',
  styleUrl: './report-cost-km.scss'
})
export class ReportCostKmComponent implements OnInit {
  private readonly reportService = inject(ReportService);
  private readonly router = inject(Router);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly cdr = inject(ChangeDetectorRef);
  readonly activeAverageOption = 'active-average';
  readonly allExpenseTypesOption = 'all-expense-types';
  readonly variableCostCategory = 2;

  readonly monthLabels = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
  rows: VehicleCostPerKmItem[] = [];
  reportYear = new Date().getFullYear();
  availableYears: number[] = [];
  selectedVehicleId = this.activeAverageOption;
  selectedExpenseTypeName = this.allExpenseTypesOption;
  isBrowser = false;
  chartVisible = false;
  loading = false;
  errorMessage = '';
  private readonly expenseTypePalette = ['#0ea5e9', '#22c55e', '#f97316', '#a855f7', '#eab308', '#ef4444', '#14b8a6', '#6366f1', '#ec4899', '#84cc16'];

  ngOnInit(): void {
    this.isBrowser = isPlatformBrowser(this.platformId);

    if (this.isBrowser) {
      queueMicrotask(() => this.load());

      this.router.events.pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd)
      ).subscribe(event => {
        if (event.urlAfterRedirects.startsWith('/Admin/reports')) {
          this.load();
        }
      });
    }
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';
    this.chartVisible = false;
    this.requestReport();
  }

  private requestReport(): void {
    this.reportService.getVehicleCostPerKm(this.reportYear).pipe(
      timeout(15000)
    ).subscribe({
      next: (data) => {
        const normalized = this.normalizeReportResponse(data);
        this.reportYear = normalized.year;
        this.availableYears = normalized.availableYears;
        this.rows = normalized.items;
        this.ensureSelectedVehicle();
        this.ensureSelectedExpenseType();
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

  onExpenseTypeChange(): void {
    this.refreshChart();
  }

  private normalizeReportResponse(data: any): { year: number; availableYears: number[]; items: VehicleCostPerKmItem[] } {
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
          annualAverageCostPerKm: Number(item?.annualAverageCostPerKm ?? item?.AnnualAverageCostPerKm ?? 0),
          months: Array.isArray(item?.months ?? item?.Months)
            ? (item?.months ?? item?.Months).map((month: any) => ({
                month: Number(month?.month ?? month?.Month ?? 0),
                totalKm: Number(month?.totalKm ?? month?.TotalKm ?? 0),
                totalExpense: Number(month?.totalExpense ?? month?.TotalExpense ?? 0),
                costPerKm: Number(month?.costPerKm ?? month?.CostPerKm ?? 0),
                expenseTypes: Array.isArray(month?.expenseTypes ?? month?.ExpenseTypes)
                  ? (month?.expenseTypes ?? month?.ExpenseTypes).map((expenseType: any) => ({
                      expenseTypeName: String(expenseType?.expenseTypeName ?? expenseType?.ExpenseTypeName ?? 'Sem Tipo'),
                      costCategory: Number(expenseType?.costCategory ?? expenseType?.CostCategory ?? this.variableCostCategory),
                      totalExpense: Number(expenseType?.totalExpense ?? expenseType?.TotalExpense ?? 0),
                      costPerKm: Number(expenseType?.costPerKm ?? expenseType?.CostPerKm ?? 0)
                    }))
                  : []
              }))
            : []
        }))
      : [];

    return { year, availableYears, items };
  }

  private ensureSelectedVehicle(): void {
    if (!this.rows.length) {
      this.selectedVehicleId = this.activeAverageOption;
      return;
    }

    if (this.selectedVehicleId === this.activeAverageOption) {
      return;
    }

    const existingVehicle = this.rows.find(row => row.vehicleId === this.selectedVehicleId);
    if (!existingVehicle) {
      this.selectedVehicleId = this.activeAverageOption;
    }
  }

  private ensureSelectedExpenseType(): void {
    if (this.selectedExpenseTypeName === this.allExpenseTypesOption) {
      return;
    }

    if (!this.expenseTypeNames.includes(this.selectedExpenseTypeName)) {
      this.selectedExpenseTypeName = this.allExpenseTypesOption;
    }
  }

  getMonthData(row: VehicleCostPerKmItem | null, month: number): VehicleCostPerKmMonthlyMetric {
    return row?.months?.find(item => item.month === month) ?? {
      month,
      totalKm: 0,
      totalExpense: 0,
      costPerKm: 0,
      expenseTypes: []
    };
  }

  get selectedVehicle(): VehicleCostPerKmItem | null {
    if (this.selectedVehicleId === this.activeAverageOption) {
      return this.activeVehiclesAverage;
    }

    return this.rows.find(row => row.vehicleId === this.selectedVehicleId) ?? null;
  }

  get activeVehicles(): VehicleCostPerKmItem[] {
    return this.rows.filter(row => row.isActive);
  }

  get activeVehiclesAverage(): VehicleCostPerKmItem | null {
    if (!this.activeVehicles.length) return null;

    const months = this.monthLabels.map((_, index) => {
      const month = index + 1;
      const metrics = this.activeVehicles.map(vehicle => this.getMonthData(vehicle, month));
      const vehicleCount = this.activeVehicles.length;
      const totalKm = metrics.reduce((sum, item) => sum + Number(item.totalKm || 0), 0);
      const totalExpense = metrics.reduce((sum, item) => sum + Number(item.totalExpense || 0), 0);
      const avgCostPerKm = vehicleCount > 0
        ? metrics.reduce((sum, item) => sum + Number(item.costPerKm || 0), 0) / vehicleCount
        : 0;

      return {
        month,
        totalKm,
        totalExpense,
        costPerKm: Number(avgCostPerKm.toFixed(4)),
        expenseTypes: this.mergeExpenseTypes(metrics)
      };
    });

    const totalKm = months.reduce((sum, item) => sum + Number(item.totalKm || 0), 0);
    const totalExpense = months.reduce((sum, item) => sum + Number(item.totalExpense || 0), 0);
    const annualAverageCostPerKm = totalKm > 0 ? totalExpense / totalKm : 0;

    return {
      vehicleId: this.activeAverageOption,
      licensePlate: 'Media',
      manufacturer: 'Todos os veiculos ativos',
      isActive: true,
      months,
      annualAverageCostPerKm: Number(annualAverageCostPerKm.toFixed(2))
    };
  }

  private mergeExpenseTypes(metrics: VehicleCostPerKmMonthlyMetric[]): VehicleCostPerKmExpenseTypeMetric[] {
    const map = new Map<string, { costCategory: number; totalExpense: number; costPerKm: number }>();

    for (const metric of metrics) {
      for (const expenseType of metric.expenseTypes ?? []) {
        const current = map.get(expenseType.expenseTypeName) ?? {
          costCategory: Number(expenseType.costCategory ?? this.variableCostCategory),
          totalExpense: 0,
          costPerKm: 0
        };
        current.totalExpense += Number(expenseType.totalExpense || 0);
        current.costPerKm += Number(expenseType.costPerKm || 0);
        map.set(expenseType.expenseTypeName, current);
      }
    }

    return Array.from(map.entries())
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([expenseTypeName, values]) => ({
        expenseTypeName,
        costCategory: Number(values.costCategory || this.variableCostCategory),
        totalExpense: Number(values.totalExpense.toFixed(2)),
        costPerKm: Number(values.costPerKm.toFixed(4))
      }));
  }

  get selectedVehicleAnnualKm(): number {
    return this.selectedVehicle?.months?.reduce((sum, item) => sum + Number(item.totalKm || 0), 0) ?? 0;
  }

  get selectedVehicleAnnualExpense(): number {
    return this.selectedVehicle?.months?.reduce((sum, item) => sum + this.getFilteredMonthExpense(item), 0) ?? 0;
  }

  get selectedVehicleAnnualAverageCostPerKm(): number {
    const totalKm = this.selectedVehicleAnnualKm;
    const totalExpense = this.selectedVehicleAnnualExpense;
    return totalKm > 0 ? totalExpense / totalKm : 0;
  }

  get selectedVehicleMaxCostPerKm(): number {
    const maxValue = this.selectedVehicle?.months?.reduce((max, item) => {
      const value = this.getFilteredMonthCostPerKm(item);
      return value > max ? value : max;
    }, 0) ?? 0;

    return maxValue > 0 ? maxValue : 1;
  }

  getBarHeight(costPerKm: number): number {
    return Math.max((Number(costPerKm || 0) / this.selectedVehicleMaxCostPerKm) * 100, 4);
  }

  get expenseTypeNames(): string[] {
    const vehicle = this.selectedVehicle;
    const monthlyData = this.monthLabels.map((_, index) => this.getMonthData(vehicle, index + 1));

    return Array.from(new Set(
      monthlyData.flatMap(item => (item.expenseTypes ?? [])
        .filter(expenseType => Number(expenseType.costCategory || this.variableCostCategory) === this.variableCostCategory)
        .map(expenseType => expenseType.expenseTypeName))
    )).sort((a, b) => a.localeCompare(b));
  }

  get filteredExpenseTypeNames(): string[] {
    if (this.selectedExpenseTypeName === this.allExpenseTypesOption) {
      return this.expenseTypeNames;
    }

    return this.expenseTypeNames.filter(name => name === this.selectedExpenseTypeName);
  }

  get expenseTypeLegend(): { name: string; color: string }[] {
    return this.filteredExpenseTypeNames.map((name, index) => ({
      name,
      color: this.getExpenseTypeColor(name, index)
    }));
  }

  private getExpenseTypeColor(name: string, fallbackIndex: number): string {
    const sourceIndex = this.expenseTypeNames.indexOf(name);
    const paletteIndex = sourceIndex >= 0 ? sourceIndex : fallbackIndex;
    return this.expenseTypePalette[paletteIndex % this.expenseTypePalette.length];
  }

  private getFilteredMonthExpense(month: VehicleCostPerKmMonthlyMetric): number {
    return (month.expenseTypes ?? [])
      .filter(item => this.isVariableExpense(item) && this.matchesExpenseTypeFilter(item))
      .reduce((sum, item) => sum + Number(item.totalExpense || 0), 0);
  }

  private getFilteredMonthCostPerKm(month: VehicleCostPerKmMonthlyMetric): number {
    return (month.expenseTypes ?? [])
      .filter(item => this.isVariableExpense(item) && this.matchesExpenseTypeFilter(item))
      .reduce((sum, item) => sum + Number(item.costPerKm || 0), 0);
  }

  private matchesExpenseTypeFilter(expenseType: VehicleCostPerKmExpenseTypeMetric): boolean {
    return this.selectedExpenseTypeName === this.allExpenseTypesOption
      || expenseType.expenseTypeName === this.selectedExpenseTypeName;
  }

  private isVariableExpense(expenseType: VehicleCostPerKmExpenseTypeMetric): boolean {
    return Number(expenseType.costCategory || this.variableCostCategory) === this.variableCostCategory;
  }

  get chartOptions(): Partial<ChartOptions> {
    const vehicle = this.selectedVehicle;
    const monthlyData = this.monthLabels.map((_, index) => this.getMonthData(vehicle, index + 1));
    const expenseTypeNames = this.filteredExpenseTypeNames;

    return {
      series: expenseTypeNames.map(expenseTypeName => ({
        name: expenseTypeName,
        data: monthlyData.map(item => {
          const expenseType = (item.expenseTypes ?? []).find(current =>
            current.expenseTypeName === expenseTypeName && this.isVariableExpense(current)
          );
          return Number(expenseType?.costPerKm || 0);
        })
      })),
      chart: {
        type: 'bar',
        height: 420,
        stacked: true,
        toolbar: {
          show: false
        },
        animations: {
          enabled: true,
          speed: 700
        },
        fontFamily: 'Segoe UI, sans-serif'
      },
      colors: expenseTypeNames.map((expenseTypeName, index) => this.getExpenseTypeColor(expenseTypeName, index)),
      plotOptions: {
        bar: {
          borderRadius: 8,
          columnWidth: '48%',
          distributed: false
        }
      },
      dataLabels: {
        enabled: false
      },
      stroke: {
        show: true,
        width: 1,
        colors: ['transparent']
      },
      fill: {
        opacity: 1,
        type: 'gradient',
        gradient: {
          shade: 'light',
          type: 'vertical',
          shadeIntensity: 0.2,
          opacityFrom: 0.95,
          opacityTo: 0.75,
          stops: [0, 100]
        }
      },
      grid: {
        borderColor: '#e2e8f0',
        strokeDashArray: 4
      },
      legend: {
        show: true,
        position: 'top',
        horizontalAlign: 'left'
      },
      xaxis: {
        categories: this.monthLabels,
        labels: {
          style: {
            colors: '#475569',
            fontSize: '12px'
          }
        },
        axisBorder: {
          color: '#cbd5e1'
        },
        axisTicks: {
          color: '#cbd5e1'
        }
      },
      yaxis: {
        labels: {
          formatter: (value: number) => `R$ ${value.toFixed(2)}`,
          style: {
            colors: '#64748b',
            fontSize: '12px'
          }
        }
      },
      tooltip: {
        theme: 'light',
        y: {
          formatter: (value: number, { dataPointIndex }: { dataPointIndex: number }) => {
            const month = monthlyData[dataPointIndex];
            const km = Number(month?.totalKm || 0).toLocaleString('pt-BR', {
              minimumFractionDigits: 2,
              maximumFractionDigits: 2
            });
            const expense = this.getFilteredMonthExpense(month).toLocaleString('pt-BR', {
              minimumFractionDigits: 2,
              maximumFractionDigits: 2
            });
            const cost = Number(value || 0).toLocaleString('pt-BR', {
              minimumFractionDigits: 2,
              maximumFractionDigits: 2
            });

            return `R$ ${cost}/km | KM: ${km} | Despesa: R$ ${expense}`;
          }
        }
      }
    };
  }
}
