import { Component, OnInit } from '@angular/core';
import { CompanyService } from '../../../services/company';
import { ExpenseTypeService } from '../../../services/expense-type';

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
    private readonly expenseTypeService: ExpenseTypeService
  ) {}

  ngOnInit(): void {
    this.loadCompanies();
  }

  loadCompanies(): void {
    this.loadingCompanies = true;
    this.errorMessage = '';

    this.companyService.getCompanies().subscribe({
      next: (companies: any[]) => {
        this.companies = (companies || []).filter((c: any) => (c.isActive ?? c.IsActive ?? true));
        this.loadingCompanies = false;

        if (this.companies.length > 0) {
          this.selectedCompanyId = this.companies[0].id || this.companies[0].Id;
          this.loadExpenseTypes();
        }
      },
      error: () => {
        this.loadingCompanies = false;
        this.errorMessage = 'Nao foi possivel carregar as empresas.';
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
      return;
    }

    this.loadingTypes = true;
    this.expenseTypeService.getExpenseTypes(this.selectedCompanyId, this.showInactive).subscribe({
      next: (res: any) => {
        const data = res?.data || res?.$values || (Array.isArray(res) ? res : []);
        this.expenseTypes = (data || []).map((item: any) => ({
          id: item.id || item.Id,
          companyId: item.companyId || item.CompanyId,
          name: item.name || item.Name,
          description: item.description || item.Description || null,
          isActive: item.isActive ?? item.IsActive ?? false
        }));
        this.loadingTypes = false;
      },
      error: () => {
        this.loadingTypes = false;
        this.errorMessage = 'Nao foi possivel carregar os tipos de despesa.';
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
    this.expenseTypeService.createExpenseType({
      companyId: this.selectedCompanyId,
      name: trimmedName,
      description: (this.description || '').trim() || null
    }).subscribe({
      next: (res: any) => {
        this.saving = false;
        this.successMessage = res?.message || 'Tipo de despesa cadastrado com sucesso.';
        this.name = '';
        this.description = '';
        this.loadExpenseTypes();
      },
      error: (err) => {
        this.saving = false;
        this.errorMessage = err?.error?.message || 'Erro ao cadastrar tipo de despesa.';
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
    this.expenseTypeService.updateExpenseType(this.editingType.id, {
      name: trimmedName,
      description: (this.editDescription || '').trim() || null
    }).subscribe({
      next: (res: any) => {
        this.savingEdit = false;
        this.successMessage = res?.message || 'Tipo de despesa atualizado com sucesso.';
        this.cancelEdit();
        this.loadExpenseTypes();
      },
      error: (err) => {
        this.savingEdit = false;
        this.errorMessage = err?.error?.message || 'Erro ao editar tipo de despesa.';
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
        this.expenseTypes = this.expenseTypes.map((item) =>
          item.id === type.id ? { ...item, isActive: nextStatus } : item
        );
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Erro ao atualizar status do tipo de despesa.';
      }
    });
  }
}
