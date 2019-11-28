import { Component, OnInit } from '@angular/core';
import * as moment from "moment";
import { AllCommunityModules, GridOptions } from '@ag-grid-community/all-modules';
import {HttpClient} from "@angular/common/http";
import {WebSocketConnectionService} from "../service/web-socket-connection-service";
import {Observable} from "rxjs";
import {Tag} from "../model/Tag";
import {formatDate} from "../util/formatters";

@Component({
  selector: 'app-tag-view',
  template: `
      <div class="row align-items-center">
          <div class="col-auto">
              <mat-form-field>
                  <input matInput placeholder="Start time" [(ngModel)]="startTime">
              </mat-form-field>
          </div>
          <div class="col-auto">
              <mat-form-field>
                  <input matInput placeholder="Count" [(ngModel)]="count">
              </mat-form-field>
          </div>
          <div class="col">
              <button mat-raised-button (click)="load()">Load</button>
          </div>
      </div>
      <div class="row flex-grow-1 flex-column">
          <ag-grid-angular
                  class="ag-theme-balham h-100"
                  [gridOptions]="gridOptions"
                  [modules]="modules" [rowData]="rowData|async">
          </ag-grid-angular>
      </div>
  `,
  host: {'class': 'flex-container'},
  styles: []
})
export class TagViewComponent implements OnInit {
  gridOptions: GridOptions = {
    columnDefs: [
      {headerName: 'TagId', field: 'tagId'},
      {headerName: 'Discover', field: 'discoveryTime', sort: 'desc', valueFormatter: v => formatDate(v.value)},
      {headerName: 'Last Seen', field: 'lastSeenTime', valueFormatter: v => formatDate(v.value)},
      {headerName: 'Count', field: 'readCount'},
      {headerName: 'Antenna', field: 'antenna'},
      {headerName: 'Rssi', field: 'rssi', valueFormatter: p => p.value.toFixed(1)},
    ],
    defaultColDef: {
      sortable: true,
      resizable: true
    },
    onGridSizeChanged: params => params.api.sizeColumnsToFit()
  };

  modules = AllCommunityModules;
  startTime: string;
  count: number = 100;
  rowData: Observable<Tag[]>;

  constructor(private http: HttpClient, ws: WebSocketConnectionService) {
    this.startTime = ws.subscriptionStartTime;
    this.load();
  }

  ngOnInit() {
  }

  load() {
    this.rowData = this.http.get<Tag[]>(`/tag?start=${this.startTime}&count=${this.count}`);
  }
}
