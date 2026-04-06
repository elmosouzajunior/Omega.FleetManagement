import { isPlatformBrowser } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { finalize } from 'rxjs';
import { CompanyService } from '../../../services/company';
import { ReceiptDocumentTypeService } from '../../../services/receipt-document-type';

type ReceiptDocumentTypeRow = {
  id: string;
  companyId: string;
  name: string;
  description: string | null;
  isActive: boolean;
};

@Component({
  selector: 'app-receipt-document-type-list',
  standalone: false,
  templateUrl: './receipt-document-type-list.html',
  styleUrl: './receipt-document-type-list.scss'
})
export class ReceiptDocumentTypeListComponent implements OnInit {
  private readonly platformId = inject(PLATFORM_ID);

  companies: any[] = [];
  items: ReceiptDocumentTypeRow[] = [];
  selectedCompanyId = '';
  showInactive = true;
  name = '';
  description = '';
  editingItem: ReceiptDocumentTypeRow | null = null;
  editName = '';
  editDescription = '';
  loadingCompanies = false;
  loadingItems = false;
  saving = false;
  savingEdit = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private readonly companyService: CompanyService,
    private readonly receiptDocumentTypeService: ReceiptDocumentTypeService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.loadCompanies();
  }

  loadCompanies(): void {
    this.loadingCompanies = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    this.companyService.getCompaniesCached().subscribe({
      next: (companies: any[]) => {
        this.companies = (companies || []).filter((item: any) => (item.isActive ?? item.IsActive ?? true));
        this.loadingCompanies = false;
        if (this.companies.length > 0) {
          this.selectedCompanyId = this.companies[0].id || this.companies[0].Id;
          this.loadItems();
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
    this.loadItems();
  }

  onShowInactiveChange(): void {
    this.loadItems();
  }

  loadItems(): void {
    if (!this.selectedCompanyId) {
      this.items = [];
      this.loadingItems = false;
      this.cdr.detectChanges();
      return;
    }

    this.loadingItems = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    this.receiptDocumentTypeService.getReceiptDocumentTypes(this.selectedCompanyId, this.showInactive).subscribe({
      next: (res: any) => {
        const data = res?.data || res?.$values || (Array.isArray(res) ? res : []);
        this.items = (data || []).map((item: any) => this.mapItem(item));
        this.loadingItems = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingItems = false;
        this.errorMessage = 'Nao foi possivel carregar os tipos de documento.';
        this.cdr.detectChanges();
      }
    });
  }

  create(): void {
    this.errorMessage = '';
    this.successMessage = '';

    const trimmedName = (this.name || '').trim();
    if (!this.selectedCompanyId || !trimmedName) {
      this.errorMessage = 'Selecione a empresa e informe o nome do tipo de documento.';
      return;
    }

    this.saving = true;
    this.cdr.detectChanges();

    this.receiptDocumentTypeService.createReceiptDocumentType({
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
        this.successMessage = res?.message || 'Tipo de documento cadastrado com sucesso.';
        const created = res?.data ? this.mapItem(res.data) : this.buildOptimisticItem(trimmedName);
        this.name = '';
        this.description = '';
        this.items = this.sortItems([created, ...this.items.filter((item) => item.id !== created.id)]);
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Erro ao cadastrar tipo de documento.';
        this.cdr.detectChanges();
      }
    });
  }

  openEdit(item: ReceiptDocumentTypeRow): void {
    this.editingItem = { ...item };
    this.editName = item.name;
    this.editDescription = item.description || '';
    this.errorMessage = '';
    this.successMessage = '';
  }

  cancelEdit(): void {
    this.editingItem = null;
    this.editName = '';
    this.editDescription = '';
  }

  saveEdit(): void {
    if (!this.editingItem?.id) return;

    const trimmedName = (this.editName || '').trim();
    if (!trimmedName) {
      this.errorMessage = 'Informe o nome do tipo de documento para editar.';
      return;
    }

    this.savingEdit = true;
    this.cdr.detectChanges();

    this.receiptDocumentTypeService.updateReceiptDocumentType(this.editingItem.id, {
      name: trimmedName,
      description: (this.editDescription || '').trim() || null
    }).pipe(
      finalize(() => {
        this.savingEdit = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (res: any) => {
        this.successMessage = res?.message || 'Tipo de documento atualizado com sucesso.';
        this.items = this.sortItems(this.items.map((item) =>
          item.id === this.editingItem?.id
            ? { ...item, name: trimmedName, description: (this.editDescription || '').trim() || null }
            : item
        ));
        this.cancelEdit();
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Erro ao editar tipo de documento.';
        this.cdr.detectChanges();
      }
    });
  }

  toggleStatus(item: ReceiptDocumentTypeRow): void {
    if (!item?.id) return;
    const nextStatus = !item.isActive;
    const question = nextStatus
      ? `Deseja ativar o tipo de documento ${item.name}?`
      : `Deseja inativar o tipo de documento ${item.name}?`;

    if (!confirm(question)) return;

    this.errorMessage = '';
    this.successMessage = '';

    this.receiptDocumentTypeService.updateStatus(item.id, nextStatus).subscribe({
      next: (res: any) => {
        this.successMessage = res?.message || (nextStatus ? 'Tipo de documento ativado com sucesso.' : 'Tipo de documento inativado com sucesso.');
        this.items = this.sortItems(this.items.map((current) =>
          current.id === item.id ? { ...current, isActive: nextStatus } : current
        ));
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Erro ao atualizar status do tipo de documento.';
        this.cdr.detectChanges();
      }
    });
  }

  private mapItem(item: any): ReceiptDocumentTypeRow {
    return {
      id: item.id || item.Id,
      companyId: item.companyId || item.CompanyId,
      name: item.name || item.Name,
      description: item.description || item.Description || null,
      isActive: item.isActive ?? item.IsActive ?? false
    };
  }

  private buildOptimisticItem(name: string): ReceiptDocumentTypeRow {
    return {
      id: crypto.randomUUID(),
      companyId: this.selectedCompanyId,
      name,
      description: (this.description || '').trim() || null,
      isActive: true
    };
  }

  private sortItems(items: ReceiptDocumentTypeRow[]): ReceiptDocumentTypeRow[] {
    return [...items].sort((left, right) => left.name.localeCompare(right.name, 'pt-BR'));
  }
}
