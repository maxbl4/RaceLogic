import {Component, OnDestroy, OnInit} from '@angular/core';
import * as moment from "moment";
import {AllCommunityModules, GridOptions, GridApi, ColumnApi, RowDataTransaction} from '@ag-grid-community/all-modules';
import {LowRpsCheckpointAggregatorService} from "../service/low-rps-checkpoint-aggregator.service";
import {Subscription} from "rxjs";

@Component({
  selector: 'app-dashboard-view',
  template: `
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
export class DashboardViewComponent implements OnInit, OnDestroy {
  private api: GridApi;
  private columnApi: ColumnApi;
  modules = AllCommunityModules;

  gridOptions: GridOptions = {
    columnDefs: [
      {headerName: 'RiderId', field: 'riderId', sort: 'asc'},
      {headerName: 'Min Rps', field: 'minRps', valueFormatter: p => p.value.toFixed(1)},
      {headerName: 'Max Rps', field: 'maxRps', valueFormatter: p => p.value.toFixed(1)},
      {headerName: 'Count', field: 'count'},
      {headerName: 'First Seen', field: 'firstSeen', valueFormatter: v => moment(v.value).format('HH:mm:ss')},
      {headerName: 'Last Seen', field: 'lastSeen', valueFormatter: v => moment(v.value).format('HH:mm:ss')},
    ],
    defaultColDef: {
      sortable: true,
      resizable: true
    },
    onGridSizeChanged: params => params.api.sizeColumnsToFit(),
    onGridReady: params => {
      this.api = params.api;
      this.columnApi = params.columnApi;
      params.api.sizeColumnsToFit();
      if (!this.subscription)
        this.subscription = this.lowRpsCheckpointAggregatorService.$updates
          .subscribe($up => {
            this.api.setRowData([]);
            if ($up)
              $up.subscribe(x => this.applyUpdate(x));
          });
    }
  };
  private subscription: Subscription;

  constructor(private lowRpsCheckpointAggregatorService: LowRpsCheckpointAggregatorService) {
  }

  ngOnInit() {
  }

  private applyUpdate(update: RowDataTransaction|null) {
    console.log(update);
    if (update) {
      this.api.updateRowData(update);
    }
  }

  ngOnDestroy(): void {
    if (this.subscription) this.subscription.unsubscribe();
  }
}
