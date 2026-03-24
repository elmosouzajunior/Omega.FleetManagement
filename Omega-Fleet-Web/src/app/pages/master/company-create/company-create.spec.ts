import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CompanyCreate } from './company-create';

describe('CompanyCreate', () => {
  let component: CompanyCreate;
  let fixture: ComponentFixture<CompanyCreate>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CompanyCreate]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CompanyCreate);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
