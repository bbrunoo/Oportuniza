import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BackgroudMenuAuxiliarComponent } from './backgroud-menu-auxiliar.component';

describe('BackgroudMenuAuxiliarComponent', () => {
  let component: BackgroudMenuAuxiliarComponent;
  let fixture: ComponentFixture<BackgroudMenuAuxiliarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BackgroudMenuAuxiliarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BackgroudMenuAuxiliarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
