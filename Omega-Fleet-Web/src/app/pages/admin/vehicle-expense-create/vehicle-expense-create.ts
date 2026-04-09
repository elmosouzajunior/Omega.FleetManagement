import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TripService } from '../../../services/trip';
import { VehicleService } from '../../../services/vehicle';

@Component({
  selector: 'app-vehicle-expense-create',
  standalone: false,
  templateUrl: './vehicle-expense-create.html',
  styleUrl: './vehicle-expense-create.scss'
})
export class VehicleExpenseCreateComponent implements OnInit {
  private readonly vehicleService = inject(VehicleService);
  private readonly tripService = inject(TripService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly fixedCostCategory = 1;

  vehicles: any[] = [];
  expenseTypes: any[] = [];

  loadingVehicles = false;
  loadingExpenseTypes = false;
  saving = false;
  errorMessage = '';
  successMessage = '';

  form = {
    vehicleId: '',
    expenseTypeId: '',
    description: '',
    value: null as number | null,
    liters: null as number | null,
    pricePerLiter: null as number | null
  };

  ngOnInit(): void {
    this.loadVehicles();
    this.loadExpenseTypes();

    const vehicleId = this.route.snapshot.queryParamMap.get('vehicleId');
    if (vehicleId) {
      this.form.vehicleId = vehicleId;
    }
  }

  loadVehicles(): void {
    this.loadingVehicles = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    this.vehicleService.getVehicles().subscribe({
      next: (res: any) => {
        this.vehicles = res?.data || res?.$values || (Array.isArray(res) ? res : []);
        this.loadingVehicles = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingVehicles = false;
        this.errorMessage = 'Nao foi possivel carregar os veiculos.';
        this.cdr.detectChanges();
      }
    });
  }

  loadExpenseTypes(): void {
    this.loadingExpenseTypes = true;
    this.cdr.detectChanges();

    this.tripService.getExpenseTypes().subscribe({
      next: (res: any) => {
        const data = res?.data || res?.$values || (Array.isArray(res) ? res : []);
        this.expenseTypes = this.filterExpenseTypesByCategory(data, this.fixedCostCategory);
        this.loadingExpenseTypes = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingExpenseTypes = false;
        this.errorMessage = 'Nao foi possivel carregar os tipos de despesa.';
        this.cdr.detectChanges();
      }
    });
  }

  submit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    const value = this.requiresLiters ? this.calculatedFuelValue : Number(this.form.value);

    if (!this.form.vehicleId || !this.form.expenseTypeId || !this.form.description || !value || value <= 0) {
      this.errorMessage = 'Preencha os campos obrigatorios com valores validos.';
      return;
    }

    if (this.requiresLiters && (!this.form.liters || this.form.liters <= 0)) {
      this.errorMessage = 'Para Combustivel ou Arla, informe os litros.';
      return;
    }

    if (this.requiresLiters && (!this.form.pricePerLiter || this.form.pricePerLiter <= 0)) {
      this.errorMessage = 'Para Combustivel ou Arla, informe o preco por litro.';
      return;
    }

    this.saving = true;

    const payload = {
      expenseTypeId: this.form.expenseTypeId,
      description: this.form.description,
      value: Number(value),
      liters: this.requiresLiters ? Number(this.form.liters) : null,
      pricePerLiter: this.requiresLiters ? Number(this.form.pricePerLiter) : null
    };

    this.vehicleService.addExpense(this.form.vehicleId, payload).subscribe({
      next: () => {
        this.saving = false;
        this.successMessage = 'Despesa do veiculo lancada com sucesso.';
        this.form = {
          vehicleId: '',
          expenseTypeId: '',
          description: '',
          value: null,
          liters: null,
          pricePerLiter: null
        };
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.saving = false;
        this.errorMessage = err?.error?.message || 'Erro ao Lançar despesa para o veiculo.';
        this.cdr.detectChanges();
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/Admin/vehicles']);
  }

  onExpenseTypeChange(): void {
    if (!this.requiresLiters) {
      this.form.liters = null;
      this.form.pricePerLiter = null;
    }
  }

  get requiresLiters(): boolean {
    const selectedType = this.expenseTypes.find((type: any) => (type.id || type.expenseTypeId) === this.form.expenseTypeId);
    const normalized = ((selectedType?.name || selectedType?.description || '') as string).trim().toLowerCase();
    return normalized.includes('combust') || normalized.includes('diesel') || normalized.includes('arla');
  }

  get calculatedFuelValue(): number | null {
    const liters = Number(this.form.liters || 0);
    const pricePerLiter = Number(this.form.pricePerLiter || 0);

    if (liters <= 0 || pricePerLiter <= 0) return null;
    return Number((liters * pricePerLiter).toFixed(2));
  }

  private filterExpenseTypesByCategory(items: any[], costCategory: number): any[] {
    return (items || []).filter((item: any) =>
      Number(item?.costCategory ?? item?.CostCategory ?? 2) === costCategory
    );
  }
}
