import { isPlatformBrowser } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { CompanyService } from '../../../services/company';
import { ExpenseTypeService } from '../../../services/expense-type';
import { finalize } from 'rxjs';

type ExpenseTypeRow = {
  id: string;
  companyId: string;
  name: string;
  description: string | null;
  isActive: boolean;
};

@Component({
  selector: 'app-expense-type-list',
  standalone: false,
  templateUrl: './expense-type-list.html',
  styleUrl: './expense-type-list.scss'
})
export class ExpenseTypeListComponent implements OnInit {
  private readonly platformId = inject(PLATFORM_ID);

  companies: any[] = [];
  expenseTypes: ExpenseTypeRow[] = [];

  selectedCompanyId = '';
  showInactive = true;

  name = '';
  description = '';

  editingType: ExpenseTypeRow | null = null;
  editName = '';
  editDescription = '';

  loadingCompanies = false;
  loadingTypes = false;
  saving = false;
  savingEdit = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private readonly companyService: CompanyService,
    private readonly expenseTypeService: ExpenseTypeService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.loadCompanies();
  }

  loadCompanies(): void {
    this.loadingCompanies = true;
    this.errorMessage = '';

    this.cdr.detectChanges();

    this.companyService.getCompaniesCached().subscribe({
      next: (companies: any[]) => {
        this.companies = (companies || []).filter((c: any) => (c.isActive ?? c.IsActive ?? true));
        this.loadingCompanies = false;

        if (this.companies.length > 0) {
          this.selectedCompanyId = this.companies[0].id || this.companies[0].Id;
          this.loadExpenseTypes();
        }

        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingCompanies = false;
        this.errorMessage = 'Nao foi possivel carregar as empresas.';
        this.cdr.detectChanges();
      }
    });
  }

  onCompanyChange(): void {
    this.loadExpenseTypes();
  }

  onShowInactiveChange(): void {
    this.loadExpenseTypes();
  }

  loadExpenseTypes(): void {
    if (!this.selectedCompanyId) {
      this.expenseTypes = [];
      this.loadingTypes = false;
      this.cdr.detectChanges();
      return;
    }

    this.loadingTypes = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    this.expenseTypeService.getExpenseTypes(this.selectedCompanyId, this.showInactive).subscribe({
      next: (res: any) => {
        const data = res?.data || res?.$values || (Array.isArray(res) ? res : []);
        this.expenseTypes = (data || []).map((item: any) => this.mapExpenseType(item));
        this.loadingTypes = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingTypes = false;
        this.errorMessage = 'Nao foi possivel carregar os tipos de despesa.';
        this.cdr.detectChanges();
      }
    });
  }

  create(): void {
    this.errorMessage = '';
    this.successMessage = '';

    const trimmedName = (this.name || '').trim();
    if (!this.selectedCompanyId || !trimmedName) {
      this.errorMessage = 'Selecione a empresa e informe o nome do tipo.';
      return;
    }

    this.saving = true;
    this.cdr.detectChanges();

    this.expenseTypeService.createExpenseType({
      companyId: this.selectedCompanyId,
      name: trimmedName,
      description: (this.description || '').trim() || null
    }).pipe(
      finalize(() => {
        this.saving = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (res: any) => {
        this.successMessage = res?.message || 'Tipo de despesa cadastrado com sucesso.';
        const createdItem = res?.data ? this.mapExpenseType(res.data) : this.buildOptimisticExpenseType(trimmedName);
        this.name = '';
        this.description = '';
        this.expenseTypes = this.sortExpenseTypes([
          createdItem,
          ...this.expenseTypes.filter((item) => item.id !== createdItem.id)
        ]);
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Erro ao cadastrar tipo de despesa.';
        this.cdr.detectChanges();
      }
    });
  }

  openEdit(type: ExpenseTypeRow): void {
    this.editingType = { ...type };
    this.editName = type.name;
    this.editDescription = type.description || '';
    this.errorMessage = '';
    this.successMessage = '';
  }

  cancelEdit(): void {
    this.editingType = null;
    this.editName = '';
    this.editDescription = '';
  }

  saveEdit(): void {
    if (!this.editingType?.id) return;

    const trimmedName = (this.editName || '').trim();
    if (!trimmedName) {
      this.errorMessage = 'Informe o nome do tipo para editar.';
      return;
    }

    this.savingEdit = true;
    this.cdr.detectChanges();

    this.expenseTypeService.updateExpenseType(this.editingType.id, {
      name: trimmedName,
      description: (this.editDescription || '').trim() || null
    }).pipe(
      finalize(() => {
        this.savingEdit = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (res: any) => {
        this.successMessage = res?.message || 'Tipo de despesa atualizado com sucesso.';
        this.expenseTypes = this.sortExpenseTypes(this.expenseTypes.map((item) =>
          item.id === this.editingType?.id
            ? {
                ...item,
                name: trimmedName,
                description: (this.editDescription || '').trim() || null
              }
            : item
        ));
        this.cancelEdit();
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Erro ao editar tipo de despesa.';
        this.cdr.detectChanges();
      }
    });
  }

  toggleStatus(type: ExpenseTypeRow): void {
    if (!type?.id) return;

    const nextStatus = !type.isActive;
    const question = nextStatus
      ? `Deseja ativar o tipo de despesa ${type.name}?`
      : `Deseja inativar o tipo de despesa ${type.name}?`;

    if (!confirm(question)) return;

    this.errorMessage = '';
    this.successMessage = '';

    this.expenseTypeService.updateStatus(type.id, nextStatus).subscribe({
      next: (res: any) => {
        this.successMessage = res?.message || (nextStatus
          ? 'Tipo de despesa ativado com sucesso.'
          : 'Tipo de despesa inativado com sucesso.');
        this.expenseTypes = this.sortExpenseTypes(this.expenseTypes.map((item) =>
          item.id === type.id ? { ...item, isActive: nextStatus } : item
        ));
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Erro ao atualizar status do tipo de despesa.';
        this.cdr.detectChanges();
      }
    });
  }

  private mapExpenseType(item: any): ExpenseTypeRow {
    return {
      id: item.id || item.Id,
      companyId: item.companyId || item.CompanyId,
      name: item.name || item.Name,
      description: item.description || item.Description || null,
      isActive: item.isActive ?? item.IsActive ?? false
    };
  }

  private buildOptimisticExpenseType(name: string): ExpenseTypeRow {
    return {
      id: crypto.randomUUID(),
      companyId: this.selectedCompanyId,
      name,
      description: (this.description || '').trim() || null,
      isActive: true
    };
  }

  private sortExpenseTypes(items: ExpenseTypeRow[]): ExpenseTypeRow[] {
    return [...items].sort((left, right) => left.name.localeCompare(right.name, 'pt-BR'));
  }
}
