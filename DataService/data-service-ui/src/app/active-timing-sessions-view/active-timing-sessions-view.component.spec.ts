import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ActiveTimingSessionsViewComponent } from './active-timing-sessions-view.component';

describe('ActiveTimingSessionsViewComponent', () => {
  let component: ActiveTimingSessionsViewComponent;
  let fixture: ComponentFixture<ActiveTimingSessionsViewComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ActiveTimingSessionsViewComponent]
    });
    fixture = TestBed.createComponent(ActiveTimingSessionsViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
