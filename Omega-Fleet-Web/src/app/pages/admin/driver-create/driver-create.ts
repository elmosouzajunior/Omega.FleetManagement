import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DriverService } from '../../../services/driver';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-driver-create',
  templateUrl: './driver-create.html',
  standalone: false
})
export class DriverCreateComponent {
  driverForm: FormGroup;

  constructor(private fb: FormBuilder, private driverService: DriverService) {
    this.driverForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      cpf: ['', [Validators.required, Validators.pattern(/^\d{11}$/)]], // Valida 11 números
      commissionRate: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit() {
    if (this.driverForm.valid) {
      this.driverService.createDriver(this.driverForm.value).subscribe({
        next: (res) => {
          alert('Motorista cadastrado com sucesso!');
          this.driverForm.reset();
        },
        error: (err) => {
          const errorMessage = err.error?.message || 'Erro ao cadastrar motorista.';
          alert('Atenção: ' + errorMessage);
        }
      });
    }
  }
}

