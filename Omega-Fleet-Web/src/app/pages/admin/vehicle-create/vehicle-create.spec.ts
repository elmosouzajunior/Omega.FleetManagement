import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VehicleCreate } from './vehicle-create';

describe('VehicleCreate', () => {
  let component: VehicleCreate;
  let fixture: ComponentFixture<VehicleCreate>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [VehicleCreate]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VehicleCreate);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
