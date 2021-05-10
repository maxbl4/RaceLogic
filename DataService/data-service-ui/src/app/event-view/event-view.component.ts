import { Component, OnInit } from '@angular/core';
import {ActivatedRoute} from "@angular/router";
import {DataClient, EventDto, SessionDto} from "../service/data-service-client";
import {GridOptions} from "ag-grid-community";
import {Observable} from "rxjs";

@Component({
  selector: 'app-event-view',
  template: `
      <mat-card class="d-flex flex-grow-1 flex-column mt-3 mb-3">
        <mat-card-header>
          <div mat-card-avatar class="example-header-image"></div>
          <mat-card-title>{{eventDto.name}}</mat-card-title>
          <mat-card-subtitle>{{eventDto.date}}</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content class="row flex-grow-1 flex-column no-gutters">
          <ag-grid-angular
            style="height: 500px"
            class="ag-theme-balham h-100"
            [gridOptions]="gridOptions"
            [rowData]="sessions|async">
          </ag-grid-angular>
        </mat-card-content>
        <mat-card-actions>
          <button mat-button>LIKE</button>
          <button mat-button>SHARE</button>
        </mat-card-actions>
      </mat-card>

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

  constructor(private route:ActivatedRoute, private dataClient: DataClient) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.id = params.get("id")!;
      this.dataClient.getEvent(this.id).subscribe(x => this.eventDto = x);
      this.sessions = this.dataClient.listSessions(this.id);
      this.dataClient.listSessions(this.id);
    });
  }
}
