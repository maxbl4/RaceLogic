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

@NgModule({
  declarations: [
    AppComponent
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
    AgGridModule
  ],
  providers: [
    DataClient, StoreClient, WebSocketConnectionService,
    {provide: DateAdapter, useClass: PickDateAdapter},
    {provide: MAT_DATE_FORMATS, useValue: PICK_FORMATS}
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
