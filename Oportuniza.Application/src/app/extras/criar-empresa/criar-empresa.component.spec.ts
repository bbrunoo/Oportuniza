import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CriarEmpresaComponent } from './criar-empresa.component';

describe('CriarEmpresaComponent', () => {
  let component: CriarEmpresaComponent;
  let fixture: ComponentFixture<CriarEmpresaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CriarEmpresaComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CriarEmpresaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
