import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TripService } from '../../../services/trip';

@Component({
  selector: 'app-trip-expense-create',
  standalone: false,
  templateUrl: './trip-expense-create.html',
  styleUrl: './trip-expense-create.scss'
})
export class TripExpenseCreateComponent implements OnInit {
  private readonly tripService = inject(TripService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly cdr = inject(ChangeDetectorRef);

  openTrips: any[] = [];
  expenseTypes: any[] = [];

  loadingTrips = false;
  loadingExpenseTypes = false;
  saving = false;
  errorMessage = '';
  successMessage = '';
  valueDisplay = '';

  form = {
    tripId: '',
    expenseTypeId: '',
    description: '',
    value: null as number | null
  };

  ngOnInit(): void {
    this.loadOpenTrips();
    this.loadExpenseTypes();

    const tripId = this.route.snapshot.queryParamMap.get('tripId');
    if (tripId) {
      this.form.tripId = tripId;
    }
  }

  loadOpenTrips(): void {
    this.loadingTrips = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    this.tripService.getTrips().subscribe({
      next: (res: any) => {
        const data = res?.data || res?.$values || (Array.isArray(res) ? res : []);
        this.openTrips = (data || []).filter((trip: any) => this.isTripOpen(trip));
        this.loadingTrips = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingTrips = false;
        this.errorMessage = 'Nao foi possivel carregar as viagens abertas.';
        this.cdr.detectChanges();
      }
    });
  }

  loadExpenseTypes(): void {
    this.loadingExpenseTypes = true;
    this.cdr.detectChanges();

    this.tripService.getExpenseTypes().subscribe({
      next: (res: any) => {
        this.expenseTypes = res?.data || res?.$values || (Array.isArray(res) ? res : []);
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

    if (!this.form.tripId || !this.form.expenseTypeId || !this.form.description || !this.form.value || this.form.value <= 0) {
      this.errorMessage = 'Preencha os campos obrigatorios com valores validos.';
      return;
    }

    this.saving = true;

    const payload = {
      expenseTypeId: this.form.expenseTypeId,
      description: this.form.description,
      value: Number(this.form.value)
    };

    this.tripService.addExpense(this.form.tripId, payload).subscribe({
      next: () => {
        this.saving = false;
        this.successMessage = 'Despesa lancada com sucesso na viagem aberta.';
        this.form = {
          tripId: '',
          expenseTypeId: '',
          description: '',
          value: null
        };
        this.valueDisplay = '';
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.saving = false;
        this.errorMessage = err?.error?.message || 'Erro ao lancar despesa na viagem.';
        this.cdr.detectChanges();
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/Admin/trips']);
  }

  onValueInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const onlyDigits = (input.value || '').replace(/\D/g, '');
    const numericValue = onlyDigits ? Number(onlyDigits) / 100 : null;

    this.form.value = numericValue;
    this.valueDisplay = numericValue === null
      ? ''
      : new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(numericValue);

    input.value = this.valueDisplay;
  }

  private isTripOpen(trip: any): boolean {
    if (trip?.status && typeof trip.status === 'string') {
      return trip.status.toLowerCase() === 'open';
    }

    if (typeof trip?.status === 'number') {
      return trip.status === 0;
    }

    if (typeof trip?.endDate === 'string') {
      return trip.endDate.startsWith('0001') || trip.endDate.trim() === '';
    }

    return !trip?.endDate;
  }
}
