import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DriverCreate } from './driver-create';

describe('DriverCreate', () => {
  let component: DriverCreate;
  let fixture: ComponentFixture<DriverCreate>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [DriverCreate]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DriverCreate);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
