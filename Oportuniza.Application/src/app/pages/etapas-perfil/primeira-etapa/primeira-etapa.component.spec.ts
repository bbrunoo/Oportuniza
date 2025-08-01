import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PrimeiraEtapaComponent } from './primeira-etapa.component';

describe('PrimeiraEtapaComponent', () => {
  let component: PrimeiraEtapaComponent;
  let fixture: ComponentFixture<PrimeiraEtapaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PrimeiraEtapaComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PrimeiraEtapaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
