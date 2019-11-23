import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import {HttpClientModule} from "@angular/common/http";
import {FormsModule} from "@angular/forms";
import {ProgressbarModule, TabsModule} from "ngx-bootstrap";
import {ConsoleLogService} from "./service/console-log.service";

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    TabsModule.forRoot(),
    ProgressbarModule.forRoot()
  ],
  providers: [ConsoleLogService],
  bootstrap: [AppComponent]
})
export class AppModule { }
