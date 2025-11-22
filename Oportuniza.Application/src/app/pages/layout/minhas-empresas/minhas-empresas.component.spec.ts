import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MinhasEmpresasComponent } from './minhas-empresas.component';

describe('MinhasEmpresasComponent', () => {
  let component: MinhasEmpresasComponent;
  let fixture: ComponentFixture<MinhasEmpresasComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MinhasEmpresasComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MinhasEmpresasComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
