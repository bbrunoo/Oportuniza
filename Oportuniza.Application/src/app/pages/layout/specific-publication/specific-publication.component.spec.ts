import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SpecificPublicationComponent } from './specific-publication.component';

describe('SpecificPublicationComponent', () => {
  let component: SpecificPublicationComponent;
  let fixture: ComponentFixture<SpecificPublicationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SpecificPublicationComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SpecificPublicationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
