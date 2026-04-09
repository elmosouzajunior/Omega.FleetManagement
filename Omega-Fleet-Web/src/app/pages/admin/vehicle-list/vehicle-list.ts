import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { Vehicle } from '../../../shared/models/vehicle';
import { VehicleService } from '../../../services/vehicle';
import { DriverService } from '../../../services/driver';
import { TripService } from '../../../services/trip';
import { Router } from '@angular/router';

@Component({
  selector: 'app-vehicle-list',
  standalone: false,
  templateUrl: './vehicle-list.html',
  styleUrl: './vehicle-list.scss'
})
export class VehicleListComponent implements OnInit {
  private vehicleService = inject(VehicleService);
  private driverService = inject(DriverService);
  private cdr = inject(ChangeDetectorRef);
  private tripService = inject(TripService);
  private router = inject(Router);
  private readonly fixedCostCategory = 1;

  allVehicles: Vehicle[] = [];
  filteredVehicles: Vehicle[] = [];
  drivers: any[] = [];

  // Estados dos Modais
  showModal = false;                // Modal de Vínculo
  showEditVehicleModal = false;     // Modal de Edição (Nome corrigido)

  selectedVehicle: Vehicle | null = null;
  vehicleToEdit: any = null;        // Variável corrigida

  driverIdToAssign: string | null = null;
  errorMessage = '';
  
  filterStatus = 'Ativo';
  loading = true;
  saving = false; // Adicionado para o estado de salvamento

  showVehicleExpenseModal = false;
  savingVehicleExpense = false;
  vehicleExpenseError = '';
  expenseTypes: any[] = [];
  vehicleForExpense: Vehicle | null = null;
  vehicleExpenseForm = {
    typeId: '',
    description: '',
    value: null as number | null,
    liters: null as number | null,
    pricePerLiter: null as number | null
  };

  ngOnInit() {
    this.loadVehicles();
  }

  loadVehicles() {
    this.loading = true;
    this.vehicleService.getVehicles().subscribe({
      next: (res: any) => {
        const data = res?.$values || (Array.isArray(res) ? res : []);
        this.allVehicles = data;
        this.applyFilters();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erro ao carregar veículos:', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // --- Lógica de Edição de Veículo ---

  openEditModal(vehicle: any) {
    this.vehicleToEdit = { ...vehicle }; // Cria cópia para edição
    this.errorMessage = '';
    this.showEditVehicleModal = true;
    this.cdr.detectChanges();
  }

  closeEditModal() {
    this.showEditVehicleModal = false;
    this.vehicleToEdit = null;
    this.errorMessage = '';
    this.cdr.detectChanges();
  }

  saveVehicleEdit() {
    if (!this.vehicleToEdit.licensePlate || !this.vehicleToEdit.manufacturer) {
      this.errorMessage = 'Placa e Marca são obrigatórios.';
      return;
    }

    this.saving = true;
    // Corrigido: Agora usando vehicleService para salvar o veículo
    this.vehicleService.update(this.vehicleToEdit.id, this.vehicleToEdit).subscribe({
      next: () => {
        this.saving = false;
        this.showEditVehicleModal = false;
        this.loadVehicles(); // Nome do método corrigido
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.saving = false;
        this.errorMessage = err.error?.message || 'Erro ao atualizar veículo.';
        this.cdr.detectChanges();
      }
    });
  }

  // --- Lógica do Modal de Vínculo ---

  openAssignDriver(vehicle: Vehicle) {
    this.selectedVehicle = vehicle;
    this.errorMessage = '';
    this.drivers = [];
    this.driverIdToAssign = (vehicle as any).driverId || (vehicle as any).DriverId || null;

    this.driverService.getDrivers().subscribe({
      next: (res: any) => {
        const data = res?.$values || (Array.isArray(res) ? res : []);
        this.drivers = data;
        this.showModal = true;
        this.cdr.detectChanges();
      },
      error: () => {
        this.errorMessage = 'Não foi possível carregar a lista de motoristas.';
        this.showModal = true;
        this.cdr.detectChanges();
      }
    });
  }

  closeModal() {
    this.showModal = false;
    this.selectedVehicle = null;
    this.driverIdToAssign = null;
    this.errorMessage = '';
    this.cdr.detectChanges();
  }

  confirmAssignment() {
    if (!this.selectedVehicle) return;

    this.loading = true;

    this.vehicleService.assignDriver(this.selectedVehicle.id!, this.driverIdToAssign)
      .subscribe({
        next: () => {
          this.loading = false;
          this.closeModal();
          this.loadVehicles();
        },
        error: (err) => {
          this.loading = false;
          this.errorMessage = err.error?.message || err.error || 'Erro na operação.';
          this.cdr.detectChanges();
        }
      });
  }


  openVehicleExpenseModal(vehicle: Vehicle) {
    this.vehicleForExpense = vehicle;
    this.vehicleExpenseError = '';
    this.vehicleExpenseForm = { typeId: '', description: '', value: null, liters: null, pricePerLiter: null };
    this.showVehicleExpenseModal = true;

    if (this.expenseTypes.length === 0) {
      this.tripService.getExpenseTypes().subscribe({
        next: (res: any) => {
          const data = res?.data || res?.$values || (Array.isArray(res) ? res : []);
          this.expenseTypes = this.filterExpenseTypesByCategory(data, this.fixedCostCategory);
          this.cdr.detectChanges();
        },
        error: () => {
          this.vehicleExpenseError = 'Não foi possível carregar os tipos de despesa.';
          this.cdr.detectChanges();
        }
      });
    }
  }

  goToVehicleExpenseScreen(vehicle: Vehicle) {
    if (!vehicle?.id) return;
    this.router.navigate(['/Admin/vehicle-expenses/new'], {
      queryParams: { vehicleId: vehicle.id }
    });
  }

  closeVehicleExpenseModal() {
    if (this.savingVehicleExpense) return;
    this.showVehicleExpenseModal = false;
    this.vehicleForExpense = null;
    this.vehicleExpenseError = '';
  }

  submitVehicleExpense() {
    if (!this.vehicleForExpense?.id) return;

    const value = this.vehicleExpenseRequiresLiters
      ? this.vehicleExpenseCalculatedValue
      : Number(this.vehicleExpenseForm.value || 0);

    if (!this.vehicleExpenseForm.typeId || !this.vehicleExpenseForm.description || !value || value <= 0) {
      this.vehicleExpenseError = 'Preencha os campos obrigatórios com valores válidos.';
      return;
    }

    if (this.vehicleExpenseRequiresLiters && (!this.vehicleExpenseForm.liters || this.vehicleExpenseForm.liters <= 0)) {
      this.vehicleExpenseError = 'Para Combustivel ou Arla, informe os litros.';
      return;
    }

    if (this.vehicleExpenseRequiresLiters && (!this.vehicleExpenseForm.pricePerLiter || this.vehicleExpenseForm.pricePerLiter <= 0)) {
      this.vehicleExpenseError = 'Para Combustivel ou Arla, informe o preco por litro.';
      return;
    }

    this.savingVehicleExpense = true;
    this.vehicleExpenseError = '';

    const payload = {
      expenseTypeId: this.vehicleExpenseForm.typeId,
      description: this.vehicleExpenseForm.description,
      value: Number(value),
      liters: this.vehicleExpenseRequiresLiters ? Number(this.vehicleExpenseForm.liters) : null,
      pricePerLiter: this.vehicleExpenseRequiresLiters ? Number(this.vehicleExpenseForm.pricePerLiter) : null
    };

    this.vehicleService.addExpense(this.vehicleForExpense.id, payload).subscribe({
      next: () => {
        this.savingVehicleExpense = false;
        this.showVehicleExpenseModal = false;
        this.vehicleForExpense = null;
        this.vehicleExpenseForm = { typeId: '', description: '', value: null, liters: null, pricePerLiter: null };
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.savingVehicleExpense = false;
        this.vehicleExpenseError = err.error?.message || 'Erro ao Lançar  despesa para o veículo.';
        this.cdr.detectChanges();
      }
    });
  }

  applyFilters() {
    const targetStatus = this.filterStatus === 'Ativo';
    this.filteredVehicles = this.allVehicles.filter(v => {
      const statusValue = v.isActive !== undefined ? v.isActive : (v as any).IsActive;
      return Boolean(statusValue) === targetStatus;
    });
    this.filteredVehicles.sort((a, b) => (a.licensePlate || '').localeCompare(b.licensePlate || ''));
    this.cdr.detectChanges();
  }

  onVehicleExpenseTypeChange(): void {
    if (!this.vehicleExpenseRequiresLiters) {
      this.vehicleExpenseForm.liters = null;
      this.vehicleExpenseForm.pricePerLiter = null;
    }
  }

  get vehicleExpenseRequiresLiters(): boolean {
    const selectedType = this.expenseTypes.find((type: any) => (type.id || type.expenseTypeId) === this.vehicleExpenseForm.typeId);
    const normalized = ((selectedType?.name || selectedType?.description || '') as string).trim().toLowerCase();
    return normalized.includes('combust') || normalized.includes('diesel') || normalized.includes('arla');
  }

  get vehicleExpenseCalculatedValue(): number | null {
    const liters = Number(this.vehicleExpenseForm.liters || 0);
    const pricePerLiter = Number(this.vehicleExpenseForm.pricePerLiter || 0);
    if (liters <= 0 || pricePerLiter <= 0) return null;
    return Number((liters * pricePerLiter).toFixed(2));
  }

  private filterExpenseTypesByCategory(items: any[], costCategory: number): any[] {
    return (items || []).filter((item: any) =>
      Number(item?.costCategory ?? item?.CostCategory ?? 2) === costCategory
    );
  }
}
