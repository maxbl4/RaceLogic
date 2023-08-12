import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import {HttpClientModule} from "@angular/common/http";
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";
import {NgxUiLoaderHttpModule, NgxUiLoaderModule} from "ngx-ui-loader";
import {MatSlideToggleModule} from "@angular/material/slide-toggle";
import {MatSelectModule} from "@angular/material/select";
import {MatInputModule} from "@angular/material/input";
import {MatDatepickerModule} from "@angular/material/datepicker";
import {MatFormFieldModule} from "@angular/material/form-field";
import {MatCardModule} from "@angular/material/card";
import {MatListModule} from "@angular/material/list";
import {MatSidenavModule} from "@angular/material/sidenav";
import {MatDividerModule} from "@angular/material/divider";
import {MatToolbarModule} from "@angular/material/toolbar";
import {MatButtonToggleModule} from "@angular/material/button-toggle";
import {MatButtonModule} from "@angular/material/button";
import {FormsModule} from "@angular/forms";
import {AgGridModule} from "ag-grid-angular";
import {DateAdapter, MAT_DATE_FORMATS} from "@angular/material/core";
import {DataClient, StoreClient} from "./service/data-service-client";
import {WebSocketConnectionService} from "./service/web-socket-connection-service";
import {PICK_FORMATS, PickDateAdapter} from "./mat-pick-date-adapter";
import {NgbModule} from "@ng-bootstrap/ng-bootstrap";
import {MainViewComponent} from "@app/main-view/main-view.component";
import {EventViewComponent} from "@app/event-view/event-view.component";
import {SessionViewComponent} from "@app/session-view/session-view.component";
import {TimingSessionAddDialogComponent} from "@app/timing-session-add-dialog/timing-session-add-dialog.component";
import {TimingSessionViewComponent} from "@app/timing-session-view/timing-session-view.component";
import {OptionsViewComponent} from "@app/options-view/options-view.component";
import {OptionsService} from "@app/service/options.service";
import {TimingSessionsService} from "@app/service/timingSessionsService";

@NgModule({
  declarations: [
    AppComponent,
    MainViewComponent,
    EventViewComponent,
    SessionViewComponent,
    TimingSessionViewComponent,
    TimingSessionAddDialogComponent,
    OptionsViewComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    NgbModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatToolbarModule,
    MatDividerModule,
    MatSidenavModule,
    MatListModule,
    MatCardModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    NgxUiLoaderModule.forRoot({
      delay: 100,
      fastFadeOut: true,
      fgsColor: "#3f51b5",
      fgsType: "folding-cube",
      overlayColor: 'transparent'
    }),
    NgxUiLoaderHttpModule.forRoot({
      delay: 100, minTime: 0, showForeground: true,
    }),
    AppRoutingModule,
    AgGridModule,
    MatListModule
  ],
  providers: [
    DataClient, StoreClient, WebSocketConnectionService,
    TimingSessionsService,
    {provide: DateAdapter, useClass: PickDateAdapter},
    {provide: MAT_DATE_FORMATS, useValue: PICK_FORMATS},
    OptionsService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
