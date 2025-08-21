import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdicionarFuncionarioComponent } from './adicionar-funcionario.component';

describe('AdicionarFuncionarioComponent', () => {
  let component: AdicionarFuncionarioComponent;
  let fixture: ComponentFixture<AdicionarFuncionarioComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdicionarFuncionarioComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdicionarFuncionarioComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
