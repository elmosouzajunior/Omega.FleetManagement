import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TripService } from '../../../services/trip';
import { VehicleService } from '../../../services/vehicle';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-trip-detail',
  standalone: false,
  templateUrl: './trip-detail.html',
  styleUrls: ['./trip-detail.scss']
})
export class TripDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly tripService = inject(TripService);
  private readonly vehicleService = inject(VehicleService);
  private readonly cdr = inject(ChangeDetectorRef);

  trip: any = null;
  tripId = '';
  isLoading = true;
  pageError = '';

  showFinishModal = false;
  savingFinish = false;
  loadingFinishMetrics = false;
  reopeningTrip = false;
  showEditOpeningModal = false;
  savingOpeningEdit = false;
  loadingVehicles = false;
  finishError = '';
  openingEditError = '';
  showEditExpenseModal = false;
  savingExpenseEdit = false;
  loadingExpenseTypes = false;
  expenseEditError = '';
  expenseTypes: any[] = [];
  availableVehicles: any[] = [];
  selectedOpeningDriverDisplay = '';
  openingLoadedWeightDisplay = '';
  finishUnloadedWeightDisplay = '';
  finishFreightValueDisplay = '';
  expenseToEdit: {
    id: string;
    expenseTypeId: string;
    description: string;
    value: number | null;
    liters: number | null;
  } | null = null;

  finishForm = {
    unloadingDate: '',
    unloadingLocation: '',
    finishKm: null as number | null,
    unloadedWeightTons: null as number | null,
    freightValue: 0,
    dieselKmPerLiter: null as number | null,
    arlaKmPerLiter: null as number | null
  };

  openingEditForm = {
    vehicleId: '',
    driverId: '',
    loadingLocation: '',
    unloadingLocation: '',
    loadingDate: '',
    startKm: null as number | null,
    tonValue: null as number | null,
    loadedWeightTons: null as number | null,
    freightValue: 0
  };

  ngOnInit(): void {
    this.tripId = this.route.snapshot.paramMap.get('id') || '';
    console.log('[TripDetail] ngOnInit - route id:', this.tripId);

    if (!this.tripId) {
      console.warn('[TripDetail] tripId ausente na rota. Redirecionando para /Admin/trips');
      this.router.navigate(['/Admin/trips']);
      return;
    }

    this.loadTrip();
  }

  loadTrip(): void {
    console.log('[TripDetail] loadTrip - iniciando carregamento para tripId:', this.tripId);
    this.isLoading = true;
    this.pageError = '';

    try {
      this.tripService.getTripById(this.tripId)
        .pipe(finalize(() => {
          this.isLoading = false;
          console.log('[TripDetail] loadTrip - finalize, isLoading:', this.isLoading);
          this.cdr.detectChanges();
        }))
        .subscribe({
          next: (res: any) => {
            console.log('[TripDetail] loadTrip - resposta recebida:', res);
            this.trip = res?.data || res;
            console.log('[TripDetail] loadTrip - trip normalizada:', this.trip);
            this.cdr.detectChanges();
          },
          error: (err) => {
            console.error('[TripDetail] loadTrip - erro HTTP:', err);
            this.pageError = err?.error?.message || 'Nao foi possivel carregar os detalhes da viagem.';
            this.cdr.detectChanges();
          }
        });
    } catch (error: any) {
      console.error('[TripDetail] loadTrip - erro síncrono:', error);
      this.isLoading = false;
      this.pageError = error?.message || 'Nao foi possivel carregar os detalhes da viagem.';
      this.cdr.detectChanges();
    }
  }

  goBack(): void {
    this.router.navigate(['/Admin/trips']);
  }

  launchExpense(): void {
    if (!this.trip?.id) return;
    this.router.navigate(['/Admin/trip-expenses/new'], {
      queryParams: { tripId: this.trip.id }
    });
  }

  openEditOpeningModal(): void {
    if (!this.trip?.id || !this.isTripOpen) return;

    this.showEditOpeningModal = true;
    this.loadingVehicles = true;
    this.openingEditError = '';
    this.selectedOpeningDriverDisplay = this.trip?.driverName || '';
    this.openingEditForm = {
      vehicleId: this.trip?.vehicleId || '',
      driverId: this.trip?.driverId || '',
      loadingLocation: this.trip?.loadingLocation || '',
      unloadingLocation: this.trip?.unloadingLocation || '',
      loadingDate: this.toDateInputValue(this.trip?.loadingDate),
      startKm: Number(this.trip?.startKm || 0),
      tonValue: Number(this.trip?.tonValue || 0),
      loadedWeightTons: Number(this.trip?.loadedWeightTons || 0),
      freightValue: Number(this.trip?.freightValue || 0)
    };
    this.openingLoadedWeightDisplay = this.formatDecimalInput(this.trip?.loadedWeightTons);

    this.vehicleService.getVehicles().subscribe({
      next: (res: any) => {
        const data = res?.$values || (Array.isArray(res) ? res : []);
        this.availableVehicles = (data || []).filter((vehicle: any) =>
          (vehicle?.driverName || '').trim() !== '' || vehicle.id === this.openingEditForm.vehicleId
        );
        this.loadingVehicles = false;
        this.syncOpeningDriverDisplay();
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingVehicles = false;
        this.openingEditError = 'Nao foi possivel carregar os veiculos.';
        this.cdr.detectChanges();
      }
    });
  }

  closeEditOpeningModal(): void {
    if (this.savingOpeningEdit) return;
    this.showEditOpeningModal = false;
    this.openingEditError = '';
    this.availableVehicles = [];
    this.selectedOpeningDriverDisplay = '';
    this.openingLoadedWeightDisplay = '';
    this.cdr.detectChanges();
  }

  onOpeningVehicleChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.openingEditForm.vehicleId = select.value;
    this.syncOpeningDriverDisplay();
  }

  submitOpeningEdit(): void {
    if (!this.trip?.id) return;

    const loadingLocation = (this.openingEditForm.loadingLocation || '').trim();
    const unloadingLocation = (this.openingEditForm.unloadingLocation || '').trim();
    const loadingDate = (this.openingEditForm.loadingDate || '').trim();
    const startKm = Number(this.openingEditForm.startKm || 0);
    const tonValue = Number(this.openingEditForm.tonValue || 0);
    const loadedWeightTons = Number(this.openingEditForm.loadedWeightTons || 0);
    const freightValue = Number(this.openingEditForm.freightValue || 0);

    if (!this.openingEditForm.vehicleId || !this.openingEditForm.driverId) {
      this.openingEditError = 'Selecione um veiculo com motorista vinculado.';
      this.cdr.detectChanges();
      return;
    }

    if (!loadingLocation || !unloadingLocation || !loadingDate) {
      this.openingEditError = 'Preencha local de carregamento, destino e data.';
      this.cdr.detectChanges();
      return;
    }

    if (startKm < 0 || tonValue <= 0 || loadedWeightTons <= 0 || freightValue <= 0) {
      this.openingEditError = 'Preencha KM inicial, valor da tonelada, peso e frete total com valores validos.';
      this.cdr.detectChanges();
      return;
    }

    this.savingOpeningEdit = true;
    this.openingEditError = '';

    this.tripService.updateOpening(this.trip.id, {
      vehicleId: this.openingEditForm.vehicleId,
      driverId: this.openingEditForm.driverId,
      loadingLocation,
      unloadingLocation,
      loadingDate,
      startKm,
      tonValue,
      loadedWeightTons,
      freightValue
    }).subscribe({
      next: () => {
        this.savingOpeningEdit = false;
        this.closeEditOpeningModal();
        this.loadTrip();
      },
      error: (err) => {
        this.savingOpeningEdit = false;
        this.openingEditError = err?.error?.message || 'Erro ao atualizar abertura da viagem.';
        this.cdr.detectChanges();
      }
    });
  }

  openEditExpenseModal(expense: any): void {
    if (!this.trip?.id) return;

    this.expenseEditError = '';
    this.expenseToEdit = {
      id: expense?.id || expense?.Id || '',
      expenseTypeId: expense?.expenseTypeId || expense?.ExpenseTypeId || '',
      description: expense?.description || expense?.Description || '',
      value: Number(expense?.value || expense?.Value || 0),
      liters: expense?.liters ?? expense?.Liters ?? null
    };
    this.showEditExpenseModal = true;

    if (this.expenseTypes.length === 0) {
      this.loadingExpenseTypes = true;
      this.tripService.getExpenseTypes().subscribe({
        next: (res: any) => {
          this.expenseTypes = res?.data || res?.$values || (Array.isArray(res) ? res : []);
          this.loadingExpenseTypes = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.loadingExpenseTypes = false;
          this.expenseEditError = 'Nao foi possivel carregar os tipos de despesa.';
          this.cdr.detectChanges();
        }
      });
    }
  }

  closeEditExpenseModal(): void {
    if (this.savingExpenseEdit) return;
    this.showEditExpenseModal = false;
    this.expenseToEdit = null;
    this.expenseEditError = '';
    this.cdr.detectChanges();
  }

  submitEditExpense(): void {
    if (!this.trip?.id || !this.expenseToEdit) return;

    const description = (this.expenseToEdit.description || '').trim();
    const value = Number(this.expenseToEdit.value);
    const liters = this.expenseToEdit.liters === null || this.expenseToEdit.liters === undefined
      ? null
      : Number(this.expenseToEdit.liters);
    const expenseTypeId = this.expenseToEdit.expenseTypeId;

    if (!expenseTypeId || !description || !value || value <= 0) {
      this.expenseEditError = 'Preencha tipo, descricao e valor validos.';
      this.cdr.detectChanges();
      return;
    }

    if (this.expenseEditRequiresLiters && (!liters || liters <= 0)) {
      this.expenseEditError = 'Para Combustivel ou Arla, informe os litros.';
      this.cdr.detectChanges();
      return;
    }

    this.savingExpenseEdit = true;
    this.expenseEditError = '';

    this.tripService.updateExpense(this.trip.id, this.expenseToEdit.id, {
      expenseTypeId,
      description,
      value,
      liters: this.expenseEditRequiresLiters ? liters : null
    }).subscribe({
      next: () => {
        this.savingExpenseEdit = false;
        this.closeEditExpenseModal();
        this.loadTrip();
      },
      error: (err) => {
        this.savingExpenseEdit = false;
        this.expenseEditError = err?.error?.message || 'Erro ao atualizar despesa.';
        this.cdr.detectChanges();
      }
    });
  }

  openFinishModal(): void {
    if (!this.isTripOpen) return;

    this.finishError = '';
    this.finishForm = {
      unloadingDate: this.nowLocalDate(),
      unloadingLocation: this.trip?.unloadingLocation || '',
      finishKm: this.trip?.startKm || null,
      unloadedWeightTons: Number(this.trip?.unloadedWeightTons ?? this.trip?.loadedWeightTons ?? 0),
      freightValue: Number(this.trip?.freightValue || 0),
      dieselKmPerLiter: this.trip?.dieselKmPerLiter ?? null,
      arlaKmPerLiter: this.trip?.arlaKmPerLiter ?? null
    };
    this.finishUnloadedWeightDisplay = this.formatDecimalInput(this.finishForm.unloadedWeightTons);
    this.syncFinishFreightValue();

    this.recalculateFinishConsumption();
    this.showFinishModal = true;
    this.refreshFinishContext();
  }

  closeFinishModal(): void {
    if (this.savingFinish) return;
    this.showFinishModal = false;
    this.finishError = '';
    this.finishUnloadedWeightDisplay = '';
    this.finishFreightValueDisplay = '';
  }

  submitFinish(): void {
    if (!this.trip?.id || this.savingFinish || this.loadingFinishMetrics) return;

    const unloadingDate = (this.finishForm.unloadingDate || '').trim();
    const unloadingLocation = (this.finishForm.unloadingLocation || '').trim() || (this.trip?.unloadingLocation || '').trim();
    const finishKm = Number(this.finishForm.finishKm);
    const unloadedWeightTons = Number(this.finishForm.unloadedWeightTons || 0);
    const freightValue = Number(this.finishForm.freightValue || 0);

    if (!unloadingDate || !finishKm || unloadedWeightTons <= 0 || freightValue <= 0) {
      this.finishError = 'Preencha data descarregamento, KM final, peso descarregamento e frete total.';
      return;
    }

    if (finishKm <= Number(this.trip?.startKm || 0)) {
      this.finishError = 'O KM final deve ser maior que o KM inicial.';
      return;
    }

    const finishDate = this.parseBrazilianDate(unloadingDate);
    if (!finishDate) {
      this.finishError = 'Data descarregamento invalida. Use DD/MM/AAAA.';
      return;
    }

    const loadingDate = new Date(this.trip?.loadingDate);
    const loadingDateOnly = new Date(loadingDate.getFullYear(), loadingDate.getMonth(), loadingDate.getDate());
    const finishDateOnly = new Date(finishDate.getFullYear(), finishDate.getMonth(), finishDate.getDate());

    if (!Number.isNaN(loadingDate.getTime()) && finishDateOnly < loadingDateOnly) {
      this.finishError = 'A data de encerramento nao pode ser anterior a data de abertura.';
      return;
    }

    this.savingFinish = true;
    this.finishError = '';

    this.tripService.finishTrip(this.trip.id, {
      unloadingDate: finishDate.toISOString(),
      unloadingLocation,
      finishKm,
      unloadedWeightTons,
      freightValue,
      dieselKmPerLiter: this.finishForm.dieselKmPerLiter,
      arlaKmPerLiter: this.finishForm.arlaKmPerLiter
    }).pipe(
      finalize(() => {
        this.savingFinish = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: () => {
        this.showFinishModal = false;
        this.loadTrip();
      },
      error: (err) => {
        this.finishError = err?.error?.message || 'Erro ao finalizar a viagem.';
        this.cdr.detectChanges();
      }
    });
  }

  cancelTrip(): void {
    if (!this.trip?.id || !this.isTripOpen) return;

    const shortId = (this.trip.id || '').toString().slice(0, 8);
    const shouldCancel = confirm(`Deseja realmente cancelar a viagem ${shortId}?`);
    if (!shouldCancel) return;

    this.tripService.cancelTrip(this.trip.id).subscribe({
      next: () => this.router.navigate(['/Admin/trips']),
      error: (err) => {
        this.pageError = err?.error?.message || 'Erro ao cancelar viagem.';
      }
    });
  }

  reopenTrip(): void {
    if (!this.trip?.id || !this.isTripClosed || this.reopeningTrip) return;

    const shortId = (this.trip.id || '').toString().slice(0, 8);
    const shouldReopen = confirm(`Deseja reabrir a viagem ${shortId}?`);
    if (!shouldReopen) return;

    this.reopeningTrip = true;
    this.pageError = '';

    this.tripService.reopenTrip(this.trip.id)
      .pipe(finalize(() => {
        this.reopeningTrip = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: () => {
          this.loadTrip();
        },
        error: (err) => {
          this.pageError = err?.error?.message || 'Erro ao reabrir viagem.';
        }
      });
  }

  get expenses(): any[] {
    if (!this.trip?.expenses) return [];
    return this.trip.expenses?.$values || (Array.isArray(this.trip.expenses) ? this.trip.expenses : []);
  }

  get totalExpenses(): number {
    return this.expenses.reduce((acc: number, curr: any) => acc + Number(curr?.value || 0), 0);
  }

  get commissionValue(): number {
    const explicitCommission = Number(this.trip?.commissionValue || 0);
    if (explicitCommission > 0) return explicitCommission;

    const freightValue = Number(this.trip?.freightValue || 0);
    const commissionPercent = Number(this.trip?.commissionPercent || 0);
    return (freightValue * commissionPercent) / 100;
  }

  get netFreight(): number {
    return Number(this.trip?.freightValue || 0) - this.totalExpenses - this.commissionValue;
  }

  get tonValue(): number {
    return Number(this.trip?.tonValue || 0);
  }

  get loadedWeightTons(): number {
    return Number(this.trip?.loadedWeightTons || 0);
  }

  get unloadedWeightTons(): number | null {
    const unloadedWeight = Number(this.trip?.unloadedWeightTons);
    return Number.isFinite(unloadedWeight) && unloadedWeight > 0 ? unloadedWeight : null;
  }

  get tripDistanceForFinish(): number {
    const finishKm = Number(this.finishForm.finishKm || 0);
    const startKm = Number(this.trip?.startKm || 0);
    if (finishKm <= 0 || finishKm <= startKm) return 0;
    return finishKm - startKm;
  }

  get expenseEditRequiresLiters(): boolean {
    if (!this.expenseToEdit?.expenseTypeId) return false;
    const selectedType = this.expenseTypes.find((type: any) => (type.id || type.expenseTypeId) === this.expenseToEdit?.expenseTypeId);
    const normalized = ((selectedType?.name || selectedType?.description || '') as string).trim().toLowerCase();
    return normalized.includes('combust') || normalized.includes('diesel') || normalized.includes('arla');
  }

  get finishWeightLossThreshold(): number {
    return this.loadedWeightTons * (1 - 0.26 / 100);
  }

  get finishFreightIsEditable(): boolean {
    const unloadedWeight = Number(this.finishForm.unloadedWeightTons || 0);
    return unloadedWeight > 0 && unloadedWeight <= this.finishWeightLossThreshold;
  }

  get finishExpectedFreightValue(): number {
    const unloadedWeight = Number(this.finishForm.unloadedWeightTons || 0);
    const baseWeight = this.finishFreightIsEditable ? unloadedWeight : this.loadedWeightTons;
    return baseWeight > 0
      ? Number((this.tonValue * baseWeight).toFixed(2))
      : Number(this.trip?.freightValue || 0);
  }

  get finishFreightRuleSummary(): string {
    const thresholdText = this.finishWeightLossThreshold.toLocaleString('pt-BR', {
      minimumFractionDigits: 3,
      maximumFractionDigits: 3
    });

    if (this.finishFreightIsEditable) {
      return `Quebra identificada. Como o peso descarregamento ficou igual ou abaixo de ${thresholdText} T, o frete pode ser recalculado pelo peso descarregado e editado manualmente.`;
    }

    return `Sem quebra acima do limite de 0,26%. O frete permanece com o valor original da viagem ate o peso descarregamento ficar igual ou abaixo de ${thresholdText} T.`;
  }

  recalculateOpeningFreight(): void {
    const tonValue = Number(this.openingEditForm.tonValue || 0);
    const loadedWeightTons = Number(this.openingEditForm.loadedWeightTons || 0);
    this.openingEditForm.freightValue = tonValue > 0 && loadedWeightTons > 0
      ? Number((tonValue * loadedWeightTons).toFixed(2))
      : 0;
  }

  onOpeningLoadedWeightChange(value: string | number): void {
    const { displayValue, numericValue } = this.normalizeDecimalInput(value);
    this.openingLoadedWeightDisplay = displayValue;
    this.openingEditForm.loadedWeightTons = numericValue;
    this.recalculateOpeningFreight();
  }

  get mileageDriven(): number {
    const finishKm = Number(this.trip?.finishKm || 0);
    const startKm = Number(this.trip?.startKm || 0);
    if (finishKm <= 0 || finishKm <= startKm) return 0;
    return finishKm - startKm;
  }

  recalculateFinishConsumption(): void {
    const tripDistance = this.tripDistanceForFinish;

    if (tripDistance <= 0) {
      this.finishForm.dieselKmPerLiter = null;
      this.finishForm.arlaKmPerLiter = null;
      return;
    }

    const dieselLiters = this.getExpenseLitersByType('diesel');
    const arlaLiters = this.getExpenseLitersByType('arla');

    this.finishForm.dieselKmPerLiter = dieselLiters > 0
      ? Number((tripDistance / dieselLiters).toFixed(2))
      : null;

    this.finishForm.arlaKmPerLiter = arlaLiters > 0
      ? Number((tripDistance / arlaLiters).toFixed(2))
      : null;
  }

  get finishConsumptionSummary(): string {
    if (this.loadingFinishMetrics) {
      return 'Atualizando despesas da viagem para recalcular automaticamente.';
    }

    const dieselLiters = this.getExpenseLitersByType('diesel');
    const arlaLiters = this.getExpenseLitersByType('arla');
    const formatLiters = (value: number) => new Intl.NumberFormat('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);

    if (this.tripDistanceForFinish <= 0) {
      if (dieselLiters <= 0 && arlaLiters <= 0) {
        return 'Informe o KM final. Nenhuma despesa de Combustivel/Arla com litros informados foi encontrada.';
      }

      const parts: string[] = [];
      if (dieselLiters > 0) parts.push(`Combustivel: ${formatLiters(dieselLiters)} L`);
      if (arlaLiters > 0) parts.push(`Arla: ${formatLiters(arlaLiters)} L`);
      return `Informe o KM final para calcular automaticamente. Litros considerados: ${parts.join(' | ')}.`;
    }

    if (dieselLiters <= 0 && arlaLiters <= 0) {
      return 'Nenhuma despesa de Combustivel/Arla com litros informados foi encontrada.';
    }

    const parts: string[] = [];
    if (dieselLiters > 0) parts.push(`Combustivel: ${formatLiters(dieselLiters)} L`);
    if (arlaLiters > 0) parts.push(`Arla: ${formatLiters(arlaLiters)} L`);
    return `Calculo automatico usando ${parts.join(' | ')}.`;
  }

  get isTripOpen(): boolean {
    if (!this.trip) return false;

    if (typeof this.trip.status === 'string') {
      const normalized = this.trip.status.toLowerCase();
      return normalized === 'open' || normalized === 'aberta';
    }

    if (typeof this.trip.status === 'number') {
      return this.trip.status === 1;
    }

    if (typeof this.trip.unloadingDate === 'string') {
      return this.trip.unloadingDate.startsWith('0001') || this.trip.unloadingDate.trim() === '';
    }

    return !this.trip.unloadingDate;
  }

  get isTripClosed(): boolean {
    return !this.isTripOpen;
  }

  private nowLocalDate(): string {
    const now = new Date();
    const pad = (value: number) => value.toString().padStart(2, '0');
    return `${pad(now.getDate())}/${pad(now.getMonth() + 1)}/${now.getFullYear()}`;
  }

  onUnloadingDateInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const digits = (input.value || '').replace(/\D/g, '').slice(0, 8);

    let masked = digits;
    if (digits.length > 2) masked = `${digits.slice(0, 2)}/${digits.slice(2)}`;
    if (digits.length > 4) masked = `${digits.slice(0, 2)}/${digits.slice(2, 4)}/${digits.slice(4)}`;

    this.finishForm.unloadingDate = masked;
    input.value = masked;
  }

  onFinishKmInput(): void {
    this.recalculateFinishConsumption();
  }

  onFinishUnloadedWeightChange(value: string | number): void {
    const { displayValue, numericValue } = this.normalizeDecimalInput(value);
    this.finishUnloadedWeightDisplay = displayValue;
    this.finishForm.unloadedWeightTons = numericValue;
    this.syncFinishFreightValue();
  }

  onFinishFreightValueChange(value: string | number): void {
    const { displayValue, numericValue } = this.normalizeCurrencyInput(value);
    this.finishFreightValueDisplay = displayValue;
    this.finishForm.freightValue = numericValue ?? 0;
  }

  private parseBrazilianDate(value: string): Date | null {
    const match = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(value);
    if (!match) return null;

    const day = Number(match[1]);
    const month = Number(match[2]);
    const year = Number(match[3]);
    const parsed = new Date(year, month - 1, day, 12, 0, 0, 0);

    if (
      parsed.getFullYear() !== year ||
      parsed.getMonth() !== month - 1 ||
      parsed.getDate() !== day
    ) {
      return null;
    }

    return parsed;
  }

  syncOpeningDriverDisplay(): void {
    const selectedVehicle = this.availableVehicles.find((vehicle: any) => vehicle.id === this.openingEditForm.vehicleId);
    this.openingEditForm.driverId = selectedVehicle?.driverId || '';
    this.selectedOpeningDriverDisplay = selectedVehicle?.driverName || 'Nenhum motorista vinculado a este veículo';
  }

  private toDateInputValue(value: string | Date | null | undefined): string {
    if (!value) return '';
    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) return '';
    return parsed.toISOString().slice(0, 10);
  }

  private formatDecimalInput(value: string | number | null | undefined): string {
    const numericValue = Number(value);
    if (!Number.isFinite(numericValue) || numericValue <= 0) return '';

    return numericValue.toLocaleString('pt-BR', {
      minimumFractionDigits: 0,
      maximumFractionDigits: 3
    });
  }

  private formatCurrencyInput(value: string | number | null | undefined): string {
    const numericValue = Number(value);
    if (!Number.isFinite(numericValue) || numericValue <= 0) return '';

    return numericValue.toLocaleString('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    });
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

  private normalizeCurrencyInput(value: string | number | null | undefined): { displayValue: string; numericValue: number | null } {
    if (value === null || value === undefined) {
      return { displayValue: '', numericValue: null };
    }

    const digits = value
      .toString()
      .trim()
      .replace(/\D/g, '');

    if (!digits) {
      return { displayValue: '', numericValue: null };
    }

    const numericValue = Number(digits) / 100;
    return {
      displayValue: numericValue.toLocaleString('pt-BR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
      }),
      numericValue
    };
  }

  private syncFinishFreightValue(): void {
    const freightValue = this.finishExpectedFreightValue;
    this.finishForm.freightValue = freightValue;
    this.finishFreightValueDisplay = this.formatCurrencyInput(freightValue);
  }

  private getExpenseLitersByType(target: 'diesel' | 'arla'): number {
    const normalizedTarget = target.toLowerCase();

    return this.expenses.reduce((total: number, expense: any) => {
      const typeName = ((expense?.expenseTypeName || expense?.type || '') as string).trim().toLowerCase();
      const liters = Number(expense?.liters || 0);

      if (liters <= 0) return total;

      if (normalizedTarget === 'diesel') {
        const isDiesel = typeName.includes('combust') || typeName.includes('diesel');
        return isDiesel ? total + liters : total;
      }

      return typeName.includes('arla') ? total + liters : total;
    }, 0);
  }

  private refreshFinishContext(): void {
    if (!this.trip?.id) return;

    this.loadingFinishMetrics = true;

    this.tripService.getTripById(this.trip.id)
      .pipe(finalize(() => {
        this.loadingFinishMetrics = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (res: any) => {
          const freshTrip = res?.data || res;
          const currentFinishKm = this.finishForm.finishKm;
          const currentUnloadingDate = this.finishForm.unloadingDate;
          const currentUnloadingLocation = this.finishForm.unloadingLocation;
          const currentUnloadedWeight = this.finishForm.unloadedWeightTons;
          const currentFreightValue = this.finishForm.freightValue;

          this.trip = freshTrip;
          this.finishForm = {
            ...this.finishForm,
            unloadingDate: currentUnloadingDate,
            unloadingLocation: currentUnloadingLocation || freshTrip?.unloadingLocation || '',
            finishKm: currentFinishKm,
            unloadedWeightTons: currentUnloadedWeight,
            freightValue: currentFreightValue
          };

          this.finishUnloadedWeightDisplay = this.formatDecimalInput(currentUnloadedWeight);
          this.syncFinishFreightValue();
          this.recalculateFinishConsumption();
          this.cdr.detectChanges();
        },
        error: () => {
          this.finishError = 'Nao foi possivel atualizar as despesas antes do fechamento.';
          this.cdr.detectChanges();
        }
      });
  }
}
