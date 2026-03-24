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
  filterStatus = 'Open'; // Open, Closed ou Todas

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

    this.filteredTrips = this.allTrips.filter(t => {
      const isOpen = this.isTripOpen(t);
      if (this.filterStatus === 'Open') return isOpen;
      if (this.filterStatus === 'Closed') return !isOpen;
      return true;
    });

    this.cdr.detectChanges();
  }

  isTripOpen(trip: any): boolean {
    if (!trip) return false;

    if (typeof trip.status === 'string') {
      const normalized = trip.status.toLowerCase();
      return normalized === 'open' || normalized === 'aberta';
    }

    if (typeof trip.status === 'number') {
      return trip.status === 1;
    }

    if (typeof trip.unloadingDate === 'string') {
      return trip.unloadingDate.startsWith('0001') || trip.unloadingDate.trim() === '';
    }

    return !trip.unloadingDate;
  }
}
