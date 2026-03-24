import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../../services/user';
import { CompanyService } from '../../../services/company';

type CompanyOption = {
  id: string;
  name: string;
  isActive: boolean;
};

@Component({
  selector: 'app-user-create',
  standalone: false,
  templateUrl: './user-create.html',
  styleUrl: './user-create.scss'
})
export class UserCreateComponent implements OnInit {
  userForm!: FormGroup;
  loading = false;
  loadingCompanies = false;
  errorMessage = '';

  companies: CompanyOption[] = [];

  constructor(
    private readonly fb: FormBuilder,
    private readonly userService: UserService,
    private readonly companyService: CompanyService,
    private readonly router: Router
  ) { }

  ngOnInit(): void {
    this.userForm = this.fb.group({
      adminFullName: ['', [Validators.required]],
      adminEmail: ['', [Validators.required, Validators.email]],
      adminPassword: ['', [Validators.required, Validators.minLength(6)]],
      companyId: ['', [Validators.required]]
    });

    this.loadCompanies();
  }

  loadCompanies(): void {
    this.loadingCompanies = true;
    this.errorMessage = '';

    this.companyService.getCompaniesCached().subscribe({
      next: (res: any) => {
        const source = Array.isArray(res) ? res : (res?.$values || res?.data || []);
        this.companies = (source || [])
          .map((company: any) => ({
            id: company.id ?? company.Id ?? '',
            name: company.name ?? company.Name ?? '',
            isActive: company.isActive ?? company.IsActive ?? false
          }))
          .filter((company: CompanyOption) => company.isActive)
          .filter((company: CompanyOption) => !!company.id && !!company.name)
          .sort((a: CompanyOption, b: CompanyOption) => a.name.localeCompare(b.name));

        this.loadingCompanies = false;
      },
      error: (err) => {
        console.error('Erro ao carregar empresas:', err);
        this.loadingCompanies = false;
        this.errorMessage = 'Nao foi possivel carregar a lista de empresas.';
      }
    });
  }

  onSubmit(): void {
    if (!this.userForm.valid || this.loading) return;

    this.loading = true;
    this.errorMessage = '';

    const payload = {
      adminFullName: this.userForm.value.adminFullName,
      adminEmail: (this.userForm.value.adminEmail || '').trim().toLowerCase(),
      adminPassword: this.userForm.value.adminPassword,
      companyId: this.userForm.value.companyId
    };

    this.userService.createUser(payload).subscribe({
      next: () => {
        alert('Administrador cadastrado com sucesso!');
        this.router.navigate(['/Master/users']);
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Erro ao cadastrar administrador.';
        this.loading = false;
      }
    });
  }

}
