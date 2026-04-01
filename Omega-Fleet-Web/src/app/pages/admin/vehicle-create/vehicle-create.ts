import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { VehicleService } from '../../../services/vehicle';

@Component({
  selector: 'app-vehicle-create',
  standalone: false,
  templateUrl: './vehicle-create.html',
  styleUrl: './vehicle-create.scss',
})

export class VehicleCreateComponent {
  vehicleForm: FormGroup;

  manufacturers = [
    { id: 1, name: 'DAF' },
    { id: 2, name: 'Iveco' },
    { id: 3, name: 'Mercedes-Benz' },
    { id: 4, name: 'Scania' },
    { id: 5, name: 'Volkswagen' },
    { id: 6, name: 'Volvo' }
  ];

  private readonly licensePlatePattern = /^[A-Za-z]{3}[0-9][A-Za-z0-9][0-9]{2}$/;

  constructor(private fb: FormBuilder, private vehicleService: VehicleService) {
    this.vehicleForm = this.fb.group({
      licensePlate: ['', [Validators.required, Validators.minLength(7), Validators.maxLength(7), Validators.pattern(this.licensePlatePattern)]],
      manufacturer: ['', [Validators.required]],
      color: ['']
    });
  }

  onLicensePlateInput(event: any) {
    const input = event.target as HTMLInputElement;
    const uppercaseValue = input.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
    this.vehicleForm.get('licensePlate')?.setValue(uppercaseValue, { emitEvent: true });
  }

  onSubmit() {
    if (this.vehicleForm.valid) {
      const vehicleData = this.vehicleForm.getRawValue();

      this.vehicleService.create(vehicleData).subscribe({
        next: () => {
          alert('Veículo cadastrado com sucesso!');
          this.vehicleForm.reset();
        },
        error: (err) => {
          console.error('Erro detalhado da API:', err);
          alert('Erro: ' + (err.error?.message || err.error?.title || 'Falha na validacao dos dados'));
        }
      });
    }
  }
}
