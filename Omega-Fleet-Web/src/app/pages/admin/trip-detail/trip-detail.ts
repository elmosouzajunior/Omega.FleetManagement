import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TripService } from '../../../services/trip';
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
  private readonly cdr = inject(ChangeDetectorRef);

  trip: any = null;
  tripId = '';
  isLoading = true;
  pageError = '';

  showFinishModal = false;
  savingFinish = false;
  reopeningTrip = false;
  finishError = '';
  showEditExpenseModal = false;
  savingExpenseEdit = false;
  loadingExpenseTypes = false;
  expenseEditError = '';
  expenseTypes: any[] = [];
  expenseToEdit: {
    id: string;
    expenseTypeId: string;
    description: string;
    value: number | null;
  } | null = null;

  finishForm = {
    unloadingDate: '',
    unloadingLocation: '',
    finishKm: null as number | null
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

  openEditExpenseModal(expense: any): void {
    if (!this.trip?.id) return;

    this.expenseEditError = '';
    this.expenseToEdit = {
      id: expense?.id || expense?.Id || '',
      expenseTypeId: expense?.expenseTypeId || expense?.ExpenseTypeId || '',
      description: expense?.description || expense?.Description || '',
      value: Number(expense?.value || expense?.Value || 0)
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
    const expenseTypeId = this.expenseToEdit.expenseTypeId;

    if (!expenseTypeId || !description || !value || value <= 0) {
      this.expenseEditError = 'Preencha tipo, descricao e valor validos.';
      this.cdr.detectChanges();
      return;
    }

    this.savingExpenseEdit = true;
    this.expenseEditError = '';

    this.tripService.updateExpense(this.trip.id, this.expenseToEdit.id, {
      expenseTypeId,
      description,
      value
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
      finishKm: this.trip?.startKm || null
    };

    this.showFinishModal = true;
  }

  closeFinishModal(): void {
    if (this.savingFinish) return;
    this.showFinishModal = false;
    this.finishError = '';
  }

  submitFinish(): void {
    if (!this.trip?.id) return;

    const unloadingDate = (this.finishForm.unloadingDate || '').trim();
    const unloadingLocation = (this.finishForm.unloadingLocation || '').trim() || (this.trip?.unloadingLocation || '').trim();
    const finishKm = Number(this.finishForm.finishKm);

    if (!unloadingDate || !finishKm) {
      this.finishError = 'Preencha data descarregamento e KM final.';
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
      finishKm
    }).subscribe({
      next: () => {
        this.savingFinish = false;
        this.showFinishModal = false;
        this.loadTrip();
      },
      error: (err) => {
        this.savingFinish = false;
        this.finishError = err?.error?.message || 'Erro ao finalizar a viagem.';
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

  get mileageDriven(): number {
    const finishKm = Number(this.trip?.finishKm || 0);
    const startKm = Number(this.trip?.startKm || 0);
    if (finishKm <= 0 || finishKm <= startKm) return 0;
    return finishKm - startKm;
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
}
