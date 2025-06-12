import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MeuspostsComponent } from './meusposts.component';

describe('MeuspostsComponent', () => {
  let component: MeuspostsComponent;
  let fixture: ComponentFixture<MeuspostsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MeuspostsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MeuspostsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
