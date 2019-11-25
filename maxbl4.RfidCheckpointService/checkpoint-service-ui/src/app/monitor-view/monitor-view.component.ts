import { Component, OnInit } from '@angular/core';
import * as moment from "moment";
import {AllCommunityModules, GridOptions, GridApi} from '@ag-grid-community/all-modules';
import {CheckpointService} from "../service/checkpoint.service";
import {Subscription} from "rxjs";
import {Checkpoint} from "../model/checkpoint";
import {map} from "rxjs/operators";

@Component({
  selector: 'app-monitor-view',
  template: `
      <div class="row">
          <div>
              <label>Show: </label>
              <mat-radio-group [(ngModel)]="display">
                  <mat-radio-button class="ml-2" [value]="displayType.regular">Regular</mat-radio-button>                  
                  <mat-radio-button class="ml-2" [value]="displayType.aggregated">Aggregated</mat-radio-button>
                  <mat-radio-button class="ml-2" [value]="displayType.lowRps">Low RPS</mat-radio-button>
                  <mat-radio-button class="ml-2" [value]="displayType.all">All</mat-radio-button>
              </mat-radio-group>
          </div>
      </div>
      <div class="row flex-grow-1 flex-column">
          <ag-grid-angular
                  class="ag-theme-balham h-100"
                  [gridOptions]="gridOptions"
                  [modules]="modules">
          </ag-grid-angular>  
      </div>
  `,
  host: {'class': 'flex-container'},
  styles: []
})
export class MonitorViewComponent implements OnInit {
  displayType = DisplayType;
  rowData: Checkpoint[] = [];
  gridOptions: GridOptions = {
    columnDefs: [
      {headerName: 'Seq', field: 'id', width: 40 },
      {headerName: 'Time', field: 'timestamp', width: 80, valueFormatter: v => moment(v.value).format('HH:mm:ss') },
      {headerName: 'RiderId', field: 'riderId'},
      {headerName: 'Count', field: 'count', width: 60}
    ],
    defaultColDef: {
      sortable: true,
      resizable: true
    },
    rowData: this.rowData,
    onGridSizeChanged: params => params.api.sizeColumnsToFit(),
    onGridReady: params => this.api = params.api
  };

  modules = AllCommunityModules;

  constructor(public checkpointService: CheckpointService) {
    this.display = DisplayType.regular;
  }

  private subscription: Subscription;
  private _display: DisplayType;
  private api: GridApi;
  get display(): DisplayType {
    return this._display;
  }

  set display(value: DisplayType) {
    this._display = value;
    if (this.subscription) this.subscription.unsubscribe();
    switch (value) {
      case DisplayType.regular:
        console.log("subscribe to all");
        this.subscription = this.checkpointService.$checkpoints
          .pipe(map(cps => cps.filter(cp => !cp.aggregated)))
          .subscribe(x => this.setRowData(x));
        break;
      case DisplayType.aggregated:
        this.subscription = this.checkpointService.getAggregateCheckpoints().subscribe(x => this.setRowData(x));
        break;
      case DisplayType.lowRps:
        this.subscription = this.checkpointService.getLowRpsCheckpoints().subscribe(x => this.setRowData(x));
        break;
      case DisplayType.all:
        console.log("subscribe to all");
        this.subscription = this.checkpointService.$checkpoints.subscribe(x => this.setRowData(x));
        break;
    }
  }

  setRowData(data:any[]){
    if (this.api) this.api.setRowData(data);
  }

  ngOnInit() {
  }
}

enum DisplayType {
  regular,
  aggregated,
  lowRps,
  all,
}
