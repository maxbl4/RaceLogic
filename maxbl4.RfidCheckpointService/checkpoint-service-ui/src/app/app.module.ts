import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {FormsModule} from "@angular/forms";
import {DemoMaterialModule} from "./material-module";
import { OptionsViewComponent } from './options-view/options-view.component';
import { MonitorViewComponent } from './monitor-view/monitor-view.component';
import { LogsViewComponent } from './logs-view/logs-view.component';
import { LowRpsViewComponent } from './low-rps-view/low-rps-view.component';
import {HttpClientModule} from "@angular/common/http";
import {WebSocketConnectionService} from "./service/web-socket-connection-service";
import {CheckpointService} from "./service/checkpoint.service";
import {ReaderStatusService} from "./service/reader-status.service";
import {OptionsService} from "./service/options.service";
import {AgGridModule} from "@ag-grid-community/angular";
import {LowRpsCheckpointAggregatorService} from "./service/low-rps-checkpoint-aggregator.service";
import { DashboardViewComponent } from './dashboard-view/dashboard-view.component';

@NgModule({
  declarations: [
    AppComponent,
    OptionsViewComponent,
    MonitorViewComponent,
    LogsViewComponent,
    LowRpsViewComponent,
    DashboardViewComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    DemoMaterialModule,
    AgGridModule.withComponents([])
  ],
  providers: [WebSocketConnectionService, CheckpointService, OptionsService, ReaderStatusService, LowRpsCheckpointAggregatorService],
  bootstrap: [AppComponent]
})
export class AppModule { }
