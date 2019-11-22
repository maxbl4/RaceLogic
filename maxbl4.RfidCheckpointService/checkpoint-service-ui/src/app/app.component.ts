import { Component } from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {RfidOptions} from "./model/rfid-options";
import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {Checkpoint} from "./model/checkpoint";
import {ReaderStatus} from "./model/reader-status";

@Component({
  selector: 'app-root',
  template: `
    <div class="container-fluid">
        <progressbar [hidden]="!isLoading" max="100" [value]="100" type="danger" [striped]="true" [animate]="true"
            style="position: absolute;left: 0px; top:0px;width: 100%"></progressbar>
        <tabset>
            <tab heading="Options">
                <div class="form-group">
                    <label>Rfid Connection String</label>
                    <input type="text" class="form-control" [(ngModel)]="rfidOptions.connectionString">
                </div>
                <div class="form-group">
                    <label>Checkpoint aggregation window milliseconds</label>
                    <input type="number" class="form-control" [(ngModel)]="rfidOptions.checkpointAggregationWindowMs">
                </div>
                <div class="form-group">
                    <label>RPS Threshold</label>
                    <input type="number" class="form-control" [(ngModel)]="rfidOptions.rpsThreshold">
                    <small>Read Per Second threshold to report poor tag reading</small>
                </div>
                <div class="form-group">
                  <div class="custom-control custom-switch">
                      <input type="checkbox" class="custom-control-input" id="customSwitch1" [(ngModel)]="rfidOptions.enabled">
                      <label class="custom-control-label" for="customSwitch1">Rfid is enabled</label>
                  </div>
                </div>
                <div class="row">
                    <div class="col-auto">
                        <button type="submit" class="btn btn-primary" (click)="saveOptions()">Save</button>                        
                    </div>
                    <div class="col-auto">
                        <button type="reset" class="btn btn-secondary" (click)="loadOptions()">Reset</button>
                    </div>
                </div>
            </tab>
            <tab heading="Basic Title 1">Basic content 1</tab>
            <tab heading="Logs">
              <div class="form-group">
                  <label>Display Log Count</label>
                  <input type="number" class="form-control" [(ngModel)]="logLineCount">
              </div>
              <div class="form-group">
                  <label>Log Filter</label>
                  <input type="text" class="form-control" [(ngModel)]="logFilter">
              </div>              
              <div class="row">
                  <pre style="width: 100%; overflow-x: scroll">{{logs}}</pre>
              </div>
            </tab>
        </tabset>
    </div>
  `,
  styles: []
})
export class AppComponent {
  title = 'checkpoint-service-ui';
  rfidOptions: RfidOptions = {};
  logs = '';
  logLineCount = 20;
  logFilter = '';
  isLoading = false;
  private connection: HubConnection;
  constructor(private http: HttpClient) {
    this.loadOptions().then();
    setInterval(() => this.updateLogs(), 1000);
    this.connection = new HubConnectionBuilder().withUrl("/ws/cp").build();
    this.connection.on("Checkpoint", (cp:Checkpoint) => console.log("checkpoint", cp));
    this.connection.on("ReaderStatus", (status:ReaderStatus) => console.log("ReaderStatus", status));
    this.connection.start().then(x => {
      this.connection.send("Subscribe", "2019-01-01").then();
    });
  }

  async loadOptions() {
    this.isLoading = true;
    this.rfidOptions = await this.http.get<RfidOptions>('options').toPromise();
    this.isLoading = false;
  }

  async saveOptions() {
    this.isLoading = true;
    await this.http.put('options', this.rfidOptions).toPromise();
    await this.loadOptions();
    this.isLoading = false;
  }

  async updateLogs() {
    let requestUri = 'log/';
    if (this.logLineCount > 0) {
      requestUri += `${this.logLineCount}/`
    }
    if (this.logFilter != null && this.logFilter.length > 0) {
      requestUri += `${this.logFilter}/`
    }
    this.logs = (await this.http.get(requestUri, {responseType: "text"}).toPromise()).toString();
  }
}
