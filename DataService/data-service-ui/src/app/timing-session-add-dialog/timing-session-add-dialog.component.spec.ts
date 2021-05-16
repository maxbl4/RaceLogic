import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimingSessionAddDialogComponent } from './timing-session-add-dialog.component';

describe('TimingSessionAddDialogComponent', () => {
  let component: TimingSessionAddDialogComponent;
  let fixture: ComponentFixture<TimingSessionAddDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ TimingSessionAddDialogComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(TimingSessionAddDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
