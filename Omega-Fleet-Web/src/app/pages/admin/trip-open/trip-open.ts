import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TripService } from '../../../services/trip';
import { VehicleService } from '../../../services/vehicle'; // Importe o seu serviço de veículos
import { DriverService } from '../../../services/driver';
import { Router } from '@angular/router';

@Component({
  selector: 'app-trip-open',
  templateUrl: './trip-open.html',
  styleUrls: ['./trip-open.scss'],
  standalone: false
})
export class TripOpenComponent implements OnInit {
  private fb = inject(FormBuilder);
  private tripService = inject(TripService);
  private vehicleService = inject(VehicleService);
  private driverService = inject(DriverService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  tripForm: FormGroup;
  selectedFile: File | null = null;

  isAdmin: boolean = false;
  currentUserId: string = '';
  availableProducts: any[] = [];
  activeVehicles: any[] = [];
  availableDrivers: any[] = [];
  selectedDriverDisplay: string = '';
  loadedWeightDisplay = '';

  constructor() {
    this.tripForm = this.fb.group({
      productId: ['', Validators.required],
      clientName: ['', Validators.required],
      vehicleId: ['', Validators.required],
      driverId: ['', Validators.required],
      commissionPercent: [null, [Validators.required, Validators.min(0), Validators.max(100)]],
      loadingLocation: ['', Validators.required],
      unloadingLocation: ['', Validators.required],
      loadingDate: [new Date().toISOString().substring(0, 10), Validators.required],
      startKm: [null, [Validators.required, Validators.min(0)]],
      tonValue: [null, [Validators.required, Validators.min(0.01)]],
      loadedWeightTons: [null, [Validators.required, Validators.min(0.01)]],
      freightValue: [null, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.checkUserRole();
  }

  private checkUserRole() {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        this.isAdmin = payload.role === 'CompanyAdmin';
        this.currentUserId = payload.UserId;

        this.loadVehicles();
        this.loadDrivers();
        this.loadProducts();

      } catch (e) {
        console.error('Erro ao ler dados do usuário', e);
      }
    }
  }

  loadVehicles() {
    this.vehicleService.getVehicles().subscribe({
      next: (res: any) => {
        const data = res?.$values || (Array.isArray(res) ? res : []);

        if (this.isAdmin) {
          // Filtra apenas veículos que têm um nome de motorista preenchido
          this.activeVehicles = data.filter((v: any) =>
            v.driverName !== null && v.driverName !== undefined && v.driverName.trim() !== ""
          );
        } else {
          // Se for motorista, comparamos o ID (certifique-se que o campo é driverId mesmo)
          this.activeVehicles = data.filter((v: any) => v.driverId === this.currentUserId);
        }

        this.cdr.detectChanges();
      }
    });
  }

  loadDrivers() {
    this.driverService.getDrivers().subscribe({
      next: (drivers: any) => {
        this.availableDrivers = drivers?.$values || (Array.isArray(drivers) ? drivers : []);
        this.cdr.detectChanges();
      }
    });
  }

  loadProducts() {
    this.tripService.getProducts().subscribe({
      next: (res: any) => {
        const data = res?.data || res?.$values || (Array.isArray(res) ? res : []);
        this.availableProducts = (data || []).filter((product: any) => (product?.isActive ?? product?.IsActive ?? true));
        this.cdr.detectChanges();
      },
      error: () => {
        this.availableProducts = [];
        this.cdr.detectChanges();
      }
    });
  }

  onVehicleChange(event: any) {
  const vehicleId = event.target.value;
  const vehicle = this.activeVehicles.find(v => v.id === vehicleId);

  if (vehicle) {
    this.selectedDriverDisplay = vehicle.driverName;

    // AQUI ESTÁ O SEGREDO: 
    // Precisamos passar o ID real para o formControl, não apenas o nome para a tela.
    this.tripForm.patchValue({ 
      vehicleId: vehicle.id,
      driverId: vehicle.driverId,
      commissionPercent: this.getDriverCommissionRates(vehicle.driverId)[0] ?? null
    });

  } else {
    this.selectedDriverDisplay = '';
    this.tripForm.patchValue({ vehicleId: '', driverId: '', commissionPercent: null });
  }
  
  this.cdr.detectChanges();
}

  private autoSelectVehicle(vehicle: any) {
    this.tripForm.patchValue({ vehicleId: vehicle.id });
    this.updateDriverField(vehicle);
  }

  private updateDriverField(vehicle: any) {
    if (vehicle && vehicle.driver) {
      this.selectedDriverDisplay = vehicle.driver.name;
      this.tripForm.patchValue({ driverId: vehicle.driver.id });
    } else {
      this.selectedDriverDisplay = 'Nenhum motorista vinculado a este veículo';
      this.tripForm.patchValue({ driverId: '' });
    }
    this.cdr.detectChanges();
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    this.selectedFile = file || null;
  }

  onSubmit() {
    if (this.tripForm.valid) {
      const data = {
        ...this.tripForm.getRawValue(),
        freightAttachment: this.selectedFile
      };

      this.tripService.openTrip(data).subscribe({
        next: () => {
          alert('Sucesso: Viagem aberta no sistema!');
          this.router.navigate(['/Admin/trips']);
        },
        error: (err) => alert('Atenção: ' + (err.error?.message || "Erro ao abrir viagem"))
      });
    }
  }

  resetForm(): void {
    this.tripForm.reset({
      productId: '',
      clientName: '',
      vehicleId: '',
      driverId: '',
      commissionPercent: null,
      loadingLocation: '',
      unloadingLocation: '',
      loadingDate: new Date().toISOString().substring(0, 10),
      startKm: null,
      tonValue: null,
      loadedWeightTons: null,
      freightValue: null
    });
    this.selectedDriverDisplay = '';
    this.loadedWeightDisplay = '';
    this.selectedFile = null;
    this.cdr.detectChanges();
  }

  formatMoney(event: any) {
    let value = event.target.value;

    // 1. Remove tudo que não for dígito
    value = value.replace(/\D/g, '');

    // 2. Converte para número e formata para a moeda brasileira
    const options = { minimumFractionDigits: 2 };
    const result = new Intl.NumberFormat('pt-BR', options).format(
      parseFloat(value) / 100
    );

    // 3. Atualiza o valor visual no input
    event.target.value = value ? result : '';

    // 4. Salva o valor numérico puro no Form para o banco (ex: 25000.00)
    const numericValue = value ? parseFloat(value) / 100 : 0;
    this.tripForm.patchValue({ freightValue: numericValue });
  }

  get selectedDriverCommissionRates(): number[] {
    const driverId = this.tripForm.get('driverId')?.value;
    return this.getDriverCommissionRates(driverId);
  }

  private getDriverCommissionRates(driverId: string | null | undefined): number[] {
    const driver = this.availableDrivers.find((item: any) => item.id === driverId);
    return driver?.commissionRates || (driver?.commissionRate !== undefined ? [driver.commissionRate] : []);
  }

  recalculateTotalFreight(): void {
    const tonValue = Number(this.tripForm.get('tonValue')?.value || 0);
    const loadedWeightTons = Number(this.tripForm.get('loadedWeightTons')?.value || 0);
    const totalFreight = tonValue > 0 && loadedWeightTons > 0
      ? Number((tonValue * loadedWeightTons).toFixed(2))
      : 0;

    this.tripForm.patchValue({ freightValue: totalFreight }, { emitEvent: false });
  }

  formatTonMoney(event: any): void {
    this.formatMoney(event);
    const numericValue = Number(this.tripForm.get('freightValue')?.value || 0);
    this.tripForm.patchValue({ tonValue: numericValue }, { emitEvent: false });
    this.recalculateTotalFreight();
  }

  onLoadedWeightChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const { displayValue, numericValue } = this.normalizeDecimalInput(input.value);
    input.value = displayValue;
    this.loadedWeightDisplay = displayValue;
    this.tripForm.patchValue({ loadedWeightTons: numericValue }, { emitEvent: false });
    this.recalculateTotalFreight();
  }

  private normalizeDecimalInput(value: string | number | null | undefined): { displayValue: string; numericValue: number | null } {
    if (value === null || value === undefined) {
      return { displayValue: '', numericValue: null };
    }

    const sanitized = value
      .toString()
      .trim()
      .replace(/\s/g, '')
      .replace(/\./g, '')
      .replace(/[^0-9,]/g, '');

    if (!sanitized) {
      return { displayValue: '', numericValue: null };
    }

    const hasComma = sanitized.includes(',');
    const [rawIntegerPart, ...fractionParts] = sanitized.split(',');
    const integerPart = (rawIntegerPart || '').replace(/,/g, '') || '0';
    const fractionPart = fractionParts.join('').slice(0, 3);
    const displayValue = hasComma
      ? `${integerPart},${fractionPart}`
      : integerPart;
    const numericValue = Number(`${integerPart}${fractionPart ? `.${fractionPart}` : ''}`);

    return {
      displayValue,
      numericValue: Number.isFinite(numericValue) ? numericValue : null
    };
  }
}
