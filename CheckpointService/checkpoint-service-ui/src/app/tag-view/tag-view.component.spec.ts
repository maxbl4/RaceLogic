import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { TagViewComponent } from './tag-view.component';

describe('TagViewComponent', () => {
  let component: TagViewComponent;
  let fixture: ComponentFixture<TagViewComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ TagViewComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TagViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
