import {Component, OnInit} from '@angular/core';
import {EventSelectorService} from "@app/service/event-selector-service";
import {DataClient, EventDto} from "@app/service/data-service-client";
import {Observable} from "rxjs";

@Component({
  selector: 'app-event-selector-view',
  template: `
    <div class="row">
      <div class="col">
        <mat-form-field class="w-100">
          <mat-label>Выбор гонки</mat-label>
          <mat-select [(ngModel)]="es.selectedEventId">
            <mat-option *ngFor="let ev of events|async" [value]="ev.id">
              {{ev.name}}
            </mat-option>
          </mat-select>
        </mat-form-field>
      </div>
    </div>
  `,
  styles: [
  ]
})
export class EventSelectorViewComponent implements OnInit{
  events?: Observable<EventDto[]>;
  constructor(public es: EventSelectorService,
              private dc: DataClient) {
  }

  ngOnInit(): void {
    this.events = this.dc.listEvents();
  }
}
