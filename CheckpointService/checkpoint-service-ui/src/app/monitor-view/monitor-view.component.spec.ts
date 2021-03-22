import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { MonitorViewComponent } from './monitor-view.component';

describe('MonitorViewComponent', () => {
  let component: MonitorViewComponent;
  let fixture: ComponentFixture<MonitorViewComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ MonitorViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MonitorViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
