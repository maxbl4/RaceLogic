import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import {DataClient, StoreClient} from "./service/data-service-client";
import {HttpClientModule} from "@angular/common/http";
import {AgGridModule} from "ag-grid-angular";
import {FormsModule} from "@angular/forms";
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";
import { MainViewComponent } from './main-view/main-view.component';
import {DemoMaterialModule} from "./material-module";
import { EventViewComponent } from './event-view/event-view.component';
import { SessionViewComponent } from './session-view/session-view.component';
import { TimingSessionViewComponent } from './timing-session-view/timing-session-view.component';

@NgModule({
  declarations: [
    AppComponent,
    MainViewComponent,
    EventViewComponent,
    SessionViewComponent,
    TimingSessionViewComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    DemoMaterialModule,
    AppRoutingModule,
    AgGridModule.withComponents([])
  ],
  providers: [DataClient, StoreClient],
  bootstrap: [AppComponent]
})
export class AppModule { }
