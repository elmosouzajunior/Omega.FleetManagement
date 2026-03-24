import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CompanyService } from '../../../services/company';

@Component({
  selector: 'app-company-create',
  standalone: false,
  templateUrl: './company-create.html',
  styleUrl: './company-create.scss'
})
export class CompanyCreateComponent implements OnInit {
  companyForm!: FormGroup;
  loading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private companyService: CompanyService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.companyForm = this.fb.group({
      // Dados da Empresa
      companyName: ['', [Validators.required, Validators.minLength(3)]],
      cnpj: ['', [Validators.required, Validators.pattern(/^\d{14}$/)]],

      // Dados do Administrador
      adminFullName: ['', [Validators.required]],
      adminEmail: ['', [Validators.required, Validators.email]],
      adminPassword: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit(): void {
    if (this.companyForm.valid) {
      this.loading = true;
      this.errorMessage = '';

      // Mapeamento: DE (Formulário Angular) -> PARA (DTO do C#)
      const payload = {
        companyName: this.companyForm.value.companyName,
        cnpj: this.companyForm.value.cnpj,
        adminFullName: this.companyForm.value.adminFullName,
        adminEmail: this.companyForm.value.adminEmail,
        AdminPassword: this.companyForm.value.adminPassword
      };

      this.companyService.createCompany(payload).subscribe({
        next: () => {
          alert('Empresa e Administrador cadastrados com sucesso!');
          this.router.navigate(['/Master/companies']);
        },
        error: (err) => {
          // Tenta pegar a mensagem real de erro do Backend
          this.errorMessage = err.error?.message || 'Erro ao cadastrar empresa.';
          this.loading = false;
        }
      });
    }
  }
}