import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { TripService } from '../../../services/trip';

@Component({
  selector: 'app-trip-list',
  standalone: false,
  templateUrl: './trip-list.html',
  styleUrl: './trip-list.scss'
})
export class TripListComponent implements OnInit {
  private tripService = inject(TripService);
  private cdr = inject(ChangeDetectorRef);

  allTrips: any[] = [];
  filteredTrips: any[] = [];
  loading = true;
  filterStatus = 'Open'; // Open, Finished, Cancelled ou Todas

  ngOnInit() {
    this.loadTrips();
  }

  loadTrips() {
    this.tripService.getTrips().subscribe({
      next: (res: any) => {
        if (res && res.success && res.data) {
          this.allTrips = res.data;
        } else {
          this.allTrips = [];
        }
        console.log('Viagens carregadas no componente:', this.allTrips);

        this.applyFilters();
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erro ao buscar viagens', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  applyFilters() {
    if (this.filterStatus === 'Todas') {
      this.filteredTrips = [...this.allTrips];
      this.cdr.detectChanges();
      return;
    }

    this.filteredTrips = this.allTrips.filter(t => this.getTripStatus(t) === this.filterStatus);

    this.cdr.detectChanges();
  }

  getTripStatus(trip: any): 'Open' | 'Finished' | 'Cancelled' {
    if (!trip) return 'Finished';

    if (typeof trip.status === 'string') {
      const normalized = trip.status.trim().toLowerCase();

      if (normalized === 'open' || normalized === 'aberta') return 'Open';
      if (normalized === 'finished' || normalized === 'encerrada') return 'Finished';
      if (normalized === 'cancelled' || normalized === 'canceled' || normalized === 'cancelada') return 'Cancelled';
    }

    if (typeof trip.status === 'number') {
      if (trip.status === 1) return 'Open';
      if (trip.status === 3) return 'Cancelled';
      return 'Finished';
    }

    if (typeof trip.unloadingDate === 'string') {
      return trip.unloadingDate.startsWith('0001') || trip.unloadingDate.trim() === ''
        ? 'Open'
        : 'Finished';
    }

    return trip.unloadingDate ? 'Finished' : 'Open';
  }

  isTripOpen(trip: any): boolean {
    return this.getTripStatus(trip) === 'Open';
  }

  getTripStatusLabel(trip: any): string {
    const status = this.getTripStatus(trip);

    if (status === 'Open') return 'ABERTA';
    if (status === 'Cancelled') return 'CANCELADA';
    return 'ENCERRADA';
  }

  getTripStatusClass(trip: any): string {
    const status = this.getTripStatus(trip);

    if (status === 'Open') return 'status-open';
    if (status === 'Cancelled') return 'status-cancelled';
    return 'status-finished';
  }
}
