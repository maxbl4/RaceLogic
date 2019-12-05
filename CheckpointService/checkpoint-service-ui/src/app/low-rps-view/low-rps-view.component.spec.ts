import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LowRpsViewComponent } from './low-rps-view.component';

describe('LowRpsViewComponent', () => {
  let component: LowRpsViewComponent;
  let fixture: ComponentFixture<LowRpsViewComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ LowRpsViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LowRpsViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
