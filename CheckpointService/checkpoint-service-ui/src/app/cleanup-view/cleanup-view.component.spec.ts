import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { CleanupViewComponent } from './cleanup-view.component';

describe('CleanupViewComponent', () => {
  let component: CleanupViewComponent;
  let fixture: ComponentFixture<CleanupViewComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ CleanupViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CleanupViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
