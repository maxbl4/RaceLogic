import { Component, OnInit } from '@angular/core';
import {ActivatedRoute} from "@angular/router";
import {DataClient, EventDto, SessionDto} from "@app/service/data-service-client";
import {GridOptions} from "ag-grid-community";
import {Observable} from "rxjs";
import {EventSelectorService} from "@app/service/event-selector-service";

@Component({
  selector: 'app-event-view',
  template: `
    <h3>{{eventDto.name}}</h3>
    <table class="table table-bordered" *ngIf="sessions">
      <thead>
        <tr>
          <th>Название</th>
          <td>Начало</td>
          <td>Мин круг</td>
          <td>Длительность</td>
        </tr>
      </thead>
      <tbody>
        <tr *ngFor="let s of sessions|async">
          <td><a [routerLink]="['session', s.id]">{{s.name}}</a></td>
          <td>{{s.startTime?.toFormat("HH:mm")}}</td>
          <td>{{s.minLap?.toFormat("mmм ssс")}}</td>
          <td>{{s.finishCriteria?.duration?.toFormat("hhч mmм ssс")}}</td>
        </tr>
      </tbody>
    </table>
<!--      <mat-card class="d-flex flex-grow-1 flex-column mt-3 mb-3">-->
<!--        <mat-card-header>-->
<!--          <div mat-card-avatar class="example-header-image"></div>-->
<!--          <mat-card-title>{{eventDto.name}}</mat-card-title>-->
<!--          <mat-card-subtitle>{{eventDto.date}}</mat-card-subtitle>-->
<!--        </mat-card-header>-->
<!--        <mat-card-content class="row flex-grow-1 flex-column no-gutters">-->
<!--          <ag-grid-angular-->
<!--            style="height: 500px"-->
<!--            class="ag-theme-balham h-100"-->
<!--            [gridOptions]="gridOptions"-->
<!--            [rowData]="sessions|async">-->
<!--          </ag-grid-angular>-->
<!--        </mat-card-content>-->
<!--        <mat-card-actions>-->
<!--          <button mat-button>LIKE</button>-->
<!--          <button mat-button>SHARE</button>-->
<!--        </mat-card-actions>-->
<!--      </mat-card>-->
  `,
  host: {'class': 'flex-container'},
  styles: [
  ]
})
export class EventViewComponent implements OnInit {
  eventDto: EventDto = new EventDto();
  private id: string = "";
  sessions: Observable<SessionDto[]>|undefined;
  gridOptions: GridOptions = {
    columnDefs: [
      {headerName: 'Название', field: 'name'},
      {headerName: 'Начало', field: 'startTime', sort: 'asc', valueFormatter: v => v.value.toFormat("dd.MM.yyyy HH:mm")},
      {headerName: 'Мин круг', field: 'minLap', valueFormatter: v => v.value.toFormat("mm:ss")},
      {headerName: 'Длительность', field: 'finishCriteria.duration', valueFormatter: v => v.value?.toFormat("hh:mm:ss")},
    ],
    defaultColDef: {
      sortable: true,
      resizable: true
    },
    onGridSizeChanged: params => params.api.sizeColumnsToFit()
  };

  constructor(private route:ActivatedRoute, private dataClient: DataClient,
              private ec: EventSelectorService) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.id = params.get("eventId") ?? this.ec.selectedEventId!;
      this.dataClient.getEvent(this.id).subscribe(x => this.eventDto = x);
      this.sessions = this.dataClient.listSessions(this.id);
    });
  }
}
