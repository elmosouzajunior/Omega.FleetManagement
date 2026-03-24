import { isPlatformBrowser } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { CompanyService } from '../../../services/company';

@Component({
  selector: 'app-company-list',
  standalone: false,
  templateUrl: './company-list.html',
  styleUrl: './company-list.scss'
})
export class CompanyListComponent implements OnInit {
  private readonly platformId = inject(PLATFORM_ID);

  companies: Array<{ id: string; name: string; cnpj: string; isActive: boolean }> = [];
  loading = true;
  saving = false;
  errorMessage = '';

  showEditModal = false;
  companyToEdit: { id: string; name: string; cnpj: string; isActive: boolean } | null = null;

  constructor(
    private companyService: CompanyService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.loadCompanies();
  }

  trackByCompanyId(_: number, company: { id: string }): string {
    return company.id;
  }

  openEditModal(company: { id: string; name: string; cnpj: string; isActive: boolean }): void {
    this.errorMessage = '';
    this.companyToEdit = { ...company };
    this.showEditModal = true;
  }

  closeEditModal(): void {
    if (this.saving) return;
    this.showEditModal = false;
    this.companyToEdit = null;
  }

  saveCompanyEdit(): void {
    if (!this.companyToEdit) return;

    const editingCompanyId = this.companyToEdit.id;
    const name = (this.companyToEdit.name || '').trim();
    const cnpj = (this.companyToEdit.cnpj || '').trim();
    const isActive = this.companyToEdit.isActive;

    if (!name || !cnpj) {
      this.errorMessage = 'Nome e CNPJ sao obrigatorios.';
      this.cdr.detectChanges();
      return;
    }

    this.saving = true;
    this.errorMessage = '';

    this.companyService.updateCompany(editingCompanyId, {
      name,
      cnpj,
      isActive
    }).subscribe({
      next: () => {
        this.companies = this.companies.map((company) =>
          company.id === editingCompanyId
            ? { ...company, name, cnpj, isActive }
            : company
        );
        this.saving = false;
        this.closeEditModal();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.saving = false;
        this.errorMessage = err?.error?.message || 'Erro ao atualizar empresa.';
        this.cdr.detectChanges();
      }
    });
  }

  private loadCompanies(): void {
    this.loading = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    this.companyService.getCompanies().subscribe({
      next: (data: any) => {
        const source = Array.isArray(data)
          ? data
          : data?.data || data?.$values || [];

        this.companies = (source || []).map((company: any) => ({
          id: company.id ?? company.Id ?? '',
          name: company.name ?? company.Name ?? '',
          cnpj: company.cnpj ?? company.Cnpj ?? '',
          isActive: company.isActive ?? company.IsActive ?? false
        }));

        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err?.error?.message || 'Erro ao buscar empresas.';
        console.error('Erro ao buscar empresas:', err);
        this.cdr.detectChanges();
      }
    });
  }
}
