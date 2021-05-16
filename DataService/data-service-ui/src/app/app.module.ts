import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import {HttpClientModule} from "@angular/common/http";
import {AgGridModule} from "ag-grid-angular";
import {FormsModule} from "@angular/forms";
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";
import {NgxUiLoaderHttpModule, NgxUiLoaderModule} from "ngx-ui-loader";
import {MatToolbarModule} from "@angular/material/toolbar";
import {MatDividerModule} from "@angular/material/divider";
import {MatSidenavModule} from "@angular/material/sidenav";
import {MatListModule} from "@angular/material/list";
import {MatButtonModule} from "@angular/material/button";
import {MatButtonToggleModule} from "@angular/material/button-toggle";
import {NgbModule} from "@ng-bootstrap/ng-bootstrap";

import { AppRoutingModule } from '@app/app-routing.module';
import { AppComponent } from '@app/app.component';
import {DataClient, StoreClient} from "@app/service/data-service-client";
import { MainViewComponent } from '@app/main-view/main-view.component';
import { EventViewComponent } from '@app/event-view/event-view.component';
import { SessionViewComponent } from '@app/session-view/session-view.component';
import { TimingSessionViewComponent } from '@app/timing-session-view/timing-session-view.component';
import { TimingSessionAddDialogComponent } from './timing-session-add-dialog/timing-session-add-dialog.component';
import { MatCardModule} from "@angular/material/card";
import {MatFormFieldModule} from "@angular/material/form-field";
import {MatSelectModule} from "@angular/material/select";
import {MatSlideToggleModule} from "@angular/material/slide-toggle";
import {MatInputModule} from "@angular/material/input";
import {MatDatepickerModule} from "@angular/material/datepicker";
import {PICK_FORMATS, PickDateAdapter} from "@app/session-view/mat-pick-date-adapter";
import {DateAdapter, MAT_DATE_FORMATS} from "@angular/material/core";


@NgModule({
  declarations: [
    AppComponent,
    MainViewComponent,
    EventViewComponent,
    SessionViewComponent,
    TimingSessionViewComponent,
    TimingSessionAddDialogComponent
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
    AgGridModule.withComponents([])
  ],
  providers: [
    DataClient, StoreClient,
    {provide: DateAdapter, useClass: PickDateAdapter},
    {provide: MAT_DATE_FORMATS, useValue: PICK_FORMATS}
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
