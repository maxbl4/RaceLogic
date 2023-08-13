import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EventSelectorViewComponent } from './event-selector-view.component';

describe('EventSelectorViewComponent', () => {
  let component: EventSelectorViewComponent;
  let fixture: ComponentFixture<EventSelectorViewComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [EventSelectorViewComponent]
    });
    fixture = TestBed.createComponent(EventSelectorViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
