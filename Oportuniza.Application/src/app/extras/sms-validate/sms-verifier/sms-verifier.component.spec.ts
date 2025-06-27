import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SmsVerifierComponent } from './sms-verifier.component';

describe('SmsVerifierComponent', () => {
  let component: SmsVerifierComponent;
  let fixture: ComponentFixture<SmsVerifierComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SmsVerifierComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SmsVerifierComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
