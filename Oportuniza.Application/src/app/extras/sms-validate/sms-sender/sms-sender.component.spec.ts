import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SmsSenderComponent } from './sms-sender.component';

describe('SmsSenderComponent', () => {
  let component: SmsSenderComponent;
  let fixture: ComponentFixture<SmsSenderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SmsSenderComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SmsSenderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
