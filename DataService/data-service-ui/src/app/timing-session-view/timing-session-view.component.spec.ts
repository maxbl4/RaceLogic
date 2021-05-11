import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimingSessionViewComponent } from './timing-session-view.component';

describe('TimingSessionViewComponent', () => {
  let component: TimingSessionViewComponent;
  let fixture: ComponentFixture<TimingSessionViewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TimingSessionViewComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TimingSessionViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
