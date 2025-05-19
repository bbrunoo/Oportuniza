import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TermosModalComponent } from './termos-modal.component';

describe('TermosModalComponent', () => {
  let component: TermosModalComponent;
  let fixture: ComponentFixture<TermosModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TermosModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TermosModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
