import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  loading = false;
  errorMessage = '';

  constructor(private fb: FormBuilder, private router: Router, private authService: AuthService) { }

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      identifier: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onLogin(): void {
    this.errorMessage = '';

    if (this.loginForm.valid) {
      this.loading = true;

      const loginData = {
        username: this.loginForm.value.identifier,
        password: this.loginForm.value.password
      };

      this.authService.login(loginData).subscribe({
        next: (response) => {
          if (response && response.token) {
            localStorage.setItem('token', response.token);

            try {
              const payloadBase64 = response.token.split('.')[1];
              const payloadJson = JSON.parse(atob(payloadBase64));
              const role = response.role || payloadJson['role'] ||
                payloadJson['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
              const fullName = response.fullName || response.FullName || payloadJson['FullName'] || '';
              const companyName = response.companyName || response.CompanyName || payloadJson['CompanyName'] || '';

              if (role === 'Master') {
                localStorage.setItem('userName', 'MASTER');
                localStorage.setItem('companyName', 'OMEGA SOLUTIONS');
              } else {
                localStorage.setItem('userName', fullName || 'ADMIN');
                if (companyName) {
                  localStorage.setItem('companyName', companyName);
                } else {
                  localStorage.removeItem('companyName');
                }
              }

              localStorage.setItem('user', JSON.stringify({
                ...response,
                role,
                fullName,
                companyName
              }));

              this.loading = false;

              if (role === 'Master') {
                this.router.navigate(['/Master/companies']);
              } else if (role === 'CompanyAdmin') {
                this.router.navigate(['/Admin/dashboard']);
              } else {
                this.router.navigate(['/login']);
              }
            } catch (error) {
              console.error('Erro ao processar login:', error);
              this.loading = false;
              this.router.navigate(['/Admin/dashboard']);
            }
          }
        },
        error: (error: unknown) => {
          this.loading = false;

          if (error instanceof Error && error.message === 'API_BASE_URL_NOT_CONFIGURED') {
            this.errorMessage = 'Configuracao da API nao encontrada no ambiente de producao.';
            return;
          }

          if (error instanceof HttpErrorResponse) {
            if (error.status === 0) {
              this.errorMessage = 'Nao foi possivel conectar com a API. Verifique a URL da API e o CORS em producao.';
              return;
            }

            this.errorMessage = error.error?.message || 'Falha ao realizar login.';
            return;
          }

          this.errorMessage = 'Falha inesperada ao realizar login.';
        }
      });
    }
  }
}
