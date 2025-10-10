import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ContextSwitcherComponent } from './context-switcher.component';

describe('ContextSwitcherComponent', () => {
  let component: ContextSwitcherComponent;
  let fixture: ComponentFixture<ContextSwitcherComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContextSwitcherComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ContextSwitcherComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
