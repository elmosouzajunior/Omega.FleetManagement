import { isPlatformBrowser } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { finalize } from 'rxjs';
import { CompanyService } from '../../../services/company';
import { ProductService } from '../../../services/product';

type ProductRow = {
  id: string;
  companyId: string;
  name: string;
  description: string | null;
  isActive: boolean;
};

@Component({
  selector: 'app-product-list',
  standalone: false,
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss'
})
export class ProductListComponent implements OnInit {
  private readonly platformId = inject(PLATFORM_ID);

  companies: any[] = [];
  products: ProductRow[] = [];
  selectedCompanyId = '';
  showInactive = true;
  name = '';
  description = '';
  editingProduct: ProductRow | null = null;
  editName = '';
  editDescription = '';
  loadingCompanies = false;
  loadingProducts = false;
  saving = false;
  savingEdit = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private readonly companyService: CompanyService,
    private readonly productService: ProductService,
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
        this.companies = (companies || []).filter((item: any) => (item.isActive ?? item.IsActive ?? true));
        this.loadingCompanies = false;

        if (this.companies.length > 0) {
          this.selectedCompanyId = this.companies[0].id || this.companies[0].Id;
          this.loadProducts();
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
    this.loadProducts();
  }

  onShowInactiveChange(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    if (!this.selectedCompanyId) {
      this.products = [];
      this.loadingProducts = false;
      this.cdr.detectChanges();
      return;
    }

    this.loadingProducts = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    this.productService.getProducts(this.selectedCompanyId, this.showInactive).subscribe({
      next: (res: any) => {
        const data = res?.data || res?.$values || (Array.isArray(res) ? res : []);
        this.products = (data || []).map((item: any) => this.mapProduct(item));
        this.loadingProducts = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loadingProducts = false;
        this.errorMessage = 'Nao foi possivel carregar os produtos.';
        this.cdr.detectChanges();
      }
    });
  }

  create(): void {
    this.errorMessage = '';
    this.successMessage = '';

    const trimmedName = (this.name || '').trim();
    if (!this.selectedCompanyId || !trimmedName) {
      this.errorMessage = 'Selecione a empresa e informe o nome do produto.';
      return;
    }

    this.saving = true;
    this.cdr.detectChanges();

    this.productService.createProduct({
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
        this.successMessage = res?.message || 'Produto cadastrado com sucesso.';
        const createdItem = res?.data ? this.mapProduct(res.data) : this.buildOptimisticProduct(trimmedName);
        this.name = '';
        this.description = '';
        this.products = this.sortProducts([
          createdItem,
          ...this.products.filter((item) => item.id !== createdItem.id)
        ]);
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Erro ao cadastrar produto.';
        this.cdr.detectChanges();
      }
    });
  }

  openEdit(product: ProductRow): void {
    this.editingProduct = { ...product };
    this.editName = product.name;
    this.editDescription = product.description || '';
    this.errorMessage = '';
    this.successMessage = '';
  }

  cancelEdit(): void {
    this.editingProduct = null;
    this.editName = '';
    this.editDescription = '';
  }

  saveEdit(): void {
    if (!this.editingProduct?.id) return;

    const trimmedName = (this.editName || '').trim();
    if (!trimmedName) {
      this.errorMessage = 'Informe o nome do produto para editar.';
      return;
    }

    this.savingEdit = true;
    this.cdr.detectChanges();

    this.productService.updateProduct(this.editingProduct.id, {
      name: trimmedName,
      description: (this.editDescription || '').trim() || null
    }).pipe(
      finalize(() => {
        this.savingEdit = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (res: any) => {
        this.successMessage = res?.message || 'Produto atualizado com sucesso.';
        this.products = this.sortProducts(this.products.map((item) =>
          item.id === this.editingProduct?.id
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
        this.errorMessage = err?.error?.message || 'Erro ao editar produto.';
        this.cdr.detectChanges();
      }
    });
  }

  toggleStatus(product: ProductRow): void {
    if (!product?.id) return;

    const nextStatus = !product.isActive;
    const question = nextStatus
      ? `Deseja ativar o produto ${product.name}?`
      : `Deseja inativar o produto ${product.name}?`;

    if (!confirm(question)) return;

    this.errorMessage = '';
    this.successMessage = '';

    this.productService.updateStatus(product.id, nextStatus).subscribe({
      next: (res: any) => {
        this.successMessage = res?.message || (nextStatus
          ? 'Produto ativado com sucesso.'
          : 'Produto inativado com sucesso.');
        this.products = this.sortProducts(this.products.map((item) =>
          item.id === product.id ? { ...item, isActive: nextStatus } : item
        ));
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Erro ao atualizar status do produto.';
        this.cdr.detectChanges();
      }
    });
  }

  private mapProduct(item: any): ProductRow {
    return {
      id: item.id || item.Id,
      companyId: item.companyId || item.CompanyId,
      name: item.name || item.Name,
      description: item.description || item.Description || null,
      isActive: item.isActive ?? item.IsActive ?? false
    };
  }

  private buildOptimisticProduct(name: string): ProductRow {
    return {
      id: crypto.randomUUID(),
      companyId: this.selectedCompanyId,
      name,
      description: (this.description || '').trim() || null,
      isActive: true
    };
  }

  private sortProducts(items: ProductRow[]): ProductRow[] {
    return [...items].sort((left, right) => left.name.localeCompare(right.name, 'pt-BR'));
  }
}
