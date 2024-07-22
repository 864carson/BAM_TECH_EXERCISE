import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AstronautResultsComponent } from './astronaut-results.component';

describe('AstronautResultsComponent', () => {
  let component: AstronautResultsComponent;
  let fixture: ComponentFixture<AstronautResultsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AstronautResultsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AstronautResultsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
