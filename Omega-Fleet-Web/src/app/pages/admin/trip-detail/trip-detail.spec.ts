import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TripDetail } from './trip-detail';

describe('TripDetail', () => {
  let component: TripDetail;
  let fixture: ComponentFixture<TripDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TripDetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TripDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
