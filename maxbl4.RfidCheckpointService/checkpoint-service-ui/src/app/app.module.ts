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
import { DashboardViewComponent } from './dashboard-view/dashboard-view.component';
import {HttpClientModule} from "@angular/common/http";

@NgModule({
  declarations: [
    AppComponent,
    OptionsViewComponent,
    MonitorViewComponent,
    LogsViewComponent,
    DashboardViewComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    DemoMaterialModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
