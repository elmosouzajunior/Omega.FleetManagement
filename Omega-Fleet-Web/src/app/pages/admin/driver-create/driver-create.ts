import { Component } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
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
      commissionRates: this.fb.array([this.createCommissionControl()]),
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  get commissionRates(): FormArray {
    return this.driverForm.get('commissionRates') as FormArray;
  }

  addCommission(): void {
    this.commissionRates.push(this.createCommissionControl());
  }

  removeCommission(index: number): void {
    if (this.commissionRates.length === 1) return;
    this.commissionRates.removeAt(index);
  }

  onSubmit() {
    if (this.driverForm.valid) {
      this.driverService.createDriver(this.driverForm.value).subscribe({
        next: (res) => {
          alert('Motorista cadastrado com sucesso!');
          this.driverForm.reset();
          while (this.commissionRates.length > 1) {
            this.commissionRates.removeAt(this.commissionRates.length - 1);
          }
          this.commissionRates.at(0).setValue(0);
        },
        error: (err) => {
          const errorMessage = err.error?.message || 'Erro ao cadastrar motorista.';
          alert('Atenção: ' + errorMessage);
        }
      });
    }
  }

  private createCommissionControl() {
    return this.fb.control(0, [Validators.required, Validators.min(0), Validators.max(100)]);
  }
}

