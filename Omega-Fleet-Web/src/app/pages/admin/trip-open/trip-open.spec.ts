import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TripOpen } from './trip-open';

describe('TripOpen', () => {
  let component: TripOpen;
  let fixture: ComponentFixture<TripOpen>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TripOpen]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TripOpen);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
