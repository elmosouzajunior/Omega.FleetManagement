import { isPlatformBrowser } from '@angular/common';
import { AfterViewInit, ChangeDetectorRef, Component, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { UserService } from '../../../services/user';

type CompanyAdminRow = {
  id: string;
  name: string;
  email: string;
  isActive: boolean;
  companyName: string;
};

@Component({
  selector: 'app-admin-list',
  standalone: false,
  templateUrl: './user-list.html',
  styleUrl: './user-list.scss'
})
export class UsersListComponent implements OnInit, AfterViewInit {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly cdr = inject(ChangeDetectorRef);

  admins: CompanyAdminRow[] = [];
  loading = false;
  saving = false;
  errorMessage = '';
  successMessage = '';

  showEditModal = false;
  adminToEdit: CompanyAdminRow | null = null;
  editForm = {
    adminFullName: '',
    adminEmail: ''
  };

  constructor(private readonly userService: UserService) { }

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    this.fetchUsers();
  }

  ngAfterViewInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;

    if (!this.loading && this.admins.length === 0) {
      setTimeout(() => this.fetchUsers(), 100);
    }
  }

  fetchUsers(forceRefresh = false): void {
    this.loading = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    this.userService.getAllUsersCached(forceRefresh).subscribe({
      next: (res: any) => {
        const source = Array.isArray(res) ? res : (res?.$values || res?.data || []);
        this.admins = (source || []).map((item: any) => ({
          id: item.id ?? item.Id ?? '',
          name: item.name ?? item.Name ?? '',
          email: item.email ?? item.Email ?? '',
          isActive: item.isActive ?? item.IsActive ?? false,
          companyName: item.companyName ?? item.CompanyName ?? '-'
        }));
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err?.error?.message || 'Erro ao carregar administradores.';
        this.cdr.detectChanges();
      }
    });
  }

  openEditModal(admin: CompanyAdminRow): void {
    this.errorMessage = '';
    this.successMessage = '';
    this.adminToEdit = { ...admin };
    this.editForm = {
      adminFullName: admin.name,
      adminEmail: (admin.email || '').trim().toLowerCase()
    };
    this.showEditModal = true;
    this.cdr.detectChanges();
  }

  closeEditModal(): void {
    if (this.saving) return;
    this.showEditModal = false;
    this.adminToEdit = null;
    this.editForm = { adminFullName: '', adminEmail: '' };
    this.cdr.detectChanges();
  }

  saveAdminEdit(): void {
    if (!this.adminToEdit?.id) return;

    const adminFullName = (this.editForm.adminFullName || '').trim();
    const adminEmail = (this.editForm.adminEmail || '').trim().toLowerCase();

    if (!adminFullName) {
      this.errorMessage = 'Nome e obrigatorio.';
      return;
    }

    if (!adminEmail) {
      this.errorMessage = 'E-mail e obrigatorio.';
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(adminEmail)) {
      this.errorMessage = 'E-mail invalido.';
      return;
    }

    this.saving = true;
    this.errorMessage = '';
    this.cdr.detectChanges();

    this.userService.updateUser(this.adminToEdit.id, { adminFullName, adminEmail }).subscribe({
      next: (res: any) => {
        this.saving = false;
        this.successMessage = res?.message || 'Administrador atualizado com sucesso.';
        this.closeEditModal();
        this.fetchUsers(true);
      },
      error: (err) => {
        this.saving = false;
        this.errorMessage = err?.error?.message || 'Erro ao atualizar administrador.';
        this.cdr.detectChanges();
      }
    });
  }

  deactivateAdmin(admin: CompanyAdminRow): void {
    if (!admin?.id || !admin.isActive) return;

    const shouldDeactivate = confirm(`Deseja desativar o administrador ${admin.name}?`);
    if (!shouldDeactivate) return;

    this.errorMessage = '';
    this.successMessage = '';

    this.userService.deactivateUser(admin.id).subscribe({
      next: (res: any) => {
        this.successMessage = res?.message || 'Administrador desativado com sucesso.';
        this.admins = this.admins.map((current) =>
          current.id === admin.id ? { ...current, isActive: false } : current
        );
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Erro ao desativar administrador.';
        this.cdr.detectChanges();
      }
    });
  }

  reactivateAdmin(admin: CompanyAdminRow): void {
    if (!admin?.id || admin.isActive) return;

    const shouldReactivate = confirm(`Deseja reativar o administrador ${admin.name}?`);
    if (!shouldReactivate) return;

    this.errorMessage = '';
    this.successMessage = '';

    this.userService.reactivateUser(admin.id).subscribe({
      next: (res: any) => {
        this.successMessage = res?.message || 'Administrador reativado com sucesso.';
        this.admins = this.admins.map((current) =>
          current.id === admin.id ? { ...current, isActive: true } : current
        );
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Erro ao reativar administrador.';
        this.cdr.detectChanges();
      }
    });
  }

}
