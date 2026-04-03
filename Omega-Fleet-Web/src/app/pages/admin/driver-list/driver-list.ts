import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { Driver } from '../../../shared/models/driver';
import { DriverService } from '../../../services/driver';

@Component({
  selector: 'app-driver-list',
  standalone: false,
  templateUrl: './driver-list.html',
  styleUrl: './driver-list.scss'
})
export class DriverListComponent implements OnInit {
  // Injeções modernas (mais seguras contra erros de 'undefined')
  private driverService = inject(DriverService);
  private cdr = inject(ChangeDetectorRef);

  showEditModal = false;
  driverToEdit: any = null;
  errorMessage = '';
  saving = false;

  allDrivers: Driver[] = [];
  filteredDrivers: Driver[] = [];
  filterStatus = 'Ativo';
  loading = true;

  // Constructor vazio agora, já que usamos inject() acima
  constructor() { }

  ngOnInit() {
    this.loadDrivers();
  }

  loadDrivers() {
    this.loading = true;
    this.driverService.getDrivers().subscribe({
      next: (res: any) => {
        // Mapeamento para garantir que pegamos o array (res ou res.$values)
        const data = res?.$values || (Array.isArray(res) ? res : []);

        console.log('Lista de motoristas recebida:', data);
        this.allDrivers = data;

        // Reset do filtro inicial
        this.filterStatus = 'Ativo';
        this.applyFilters();

        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Erro ao carregar motoristas:', err);
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  applyFilters() {
    const targetStatus = this.filterStatus === 'Ativo';

    this.filteredDrivers = this.allDrivers.filter(d => {
      // Tenta pegar o status em camelCase ou PascalCase
      const statusValue = d.isActive !== undefined ? d.isActive : (d as any).IsActive;
      return Boolean(statusValue) === targetStatus;
    });

    // Ordenação alfabética
    this.filteredDrivers.sort((a, b) => (a.name || '').localeCompare(b.name || ''));

    // Agora o cdr.detectChanges() não falhará mais
    this.cdr.detectChanges();
  }

  // Método para abrir o modal de edição
  openEditModal(driver: any) {
    this.driverToEdit = { ...driver }; // Cópia para não alterar a tabela antes do OK
    this.driverToEdit.commissionRates = [...(driver.commissionRates || [driver.commissionRate])];
    this.errorMessage = '';
    this.showEditModal = true;
    this.cdr.detectChanges();
  }

  // Método para salvar no backend
  saveDriverEdit() {
    if (!this.driverToEdit.name || !this.driverToEdit.cpf) {
      this.errorMessage = 'Nome e CPF são obrigatórios.';
      return;
    }

    if (!Array.isArray(this.driverToEdit.commissionRates) || this.driverToEdit.commissionRates.length === 0) {
      this.errorMessage = 'Informe ao menos uma comissão.';
      return;
    }

    this.saving = true;
    this.driverService.update(this.driverToEdit.id, this.driverToEdit).subscribe({
      next: () => {
        this.saving = false;
        this.showEditModal = false;
        this.loadDrivers(); // Recarrega a lista
      },
      error: (err: any) => {
        this.saving = false;
        this.errorMessage = err.error?.message || 'Erro ao atualizar motorista.';
        this.cdr.detectChanges();
      }
    });
  }

  addCommissionToEdit(): void {
    if (!this.driverToEdit) return;
    if (!Array.isArray(this.driverToEdit.commissionRates)) {
      this.driverToEdit.commissionRates = [];
    }

    this.driverToEdit.commissionRates.push(0);
  }

  removeCommissionFromEdit(index: number): void {
    if (!this.driverToEdit?.commissionRates || this.driverToEdit.commissionRates.length === 1) return;
    this.driverToEdit.commissionRates.splice(index, 1);
  }
}
