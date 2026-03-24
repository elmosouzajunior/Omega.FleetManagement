import { Component, OnInit, inject, PLATFORM_ID, ChangeDetectorRef } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { DashboardService } from '../../../services/admin-dashboard';

@Component({
  selector: 'app-admin-dashboard',
  standalone: false,
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss'
})
export class AdminDashboardComponent implements OnInit {
  private platformId = inject(PLATFORM_ID);
  private cdr = inject(ChangeDetectorRef);
  private dashboardService = inject(DashboardService);

  // --- NOVAS PROPRIEDADES PARA O LAYOUT ---
  isSidebarOpen: boolean = true; // Controla a exibição da Sidebar
  userName: string = 'Usuário';  // Armazena o nome para o componente app-sidebar

  stats = {
    activeDrivers: 0,
    activeVehicles: 0,
    openTrips: 0
  };

  pendingExpenses: any[] = [];
  loading = true;

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadUserData(); // Carrega o nome do usuário logado
      this.loadDashboardData();
    }
  }

  /**
   * Recupera o nome do usuário do localStorage de forma segura
   */
  private loadUserData() {
    try {
      const user = JSON.parse(localStorage.getItem('user') || '{}');
      this.userName = user.name || 'Usuário';
    } catch (e) {
      this.userName = 'Usuário';
    }
  }

  loadDashboardData() {
    this.dashboardService.getAdminStats().subscribe({
      next: (res: any) => {
        console.log('Dados recebidos no refresh:', res);

        this.stats = {
          activeDrivers: res.activeDrivers || 0,
          activeVehicles: res.activeVehicles || 0,
          openTrips: res.openTrips || 0
        };

        this.pendingExpenses = res.pendingExpenses || res.$values || [];
        this.loading = false;
        
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erro ao carregar dashboard:', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  // Método opcional caso queira lógica extra ao fechar/abrir
  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
    this.cdr.detectChanges();
  }
}