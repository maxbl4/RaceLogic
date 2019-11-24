import { Component, OnInit } from '@angular/core';
import {RfidOptions} from "../model/rfid-options";
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-options-view',
  template: `
      <mat-form-field class="w-100">
          <input matInput placeholder="Rfid Connection String" [(ngModel)]="rfidOptions.connectionString">
      </mat-form-field>
      <mat-form-field class="w-100">
          <input matInput placeholder="Checkpoint aggregation window milliseconds"
                 type="number" [(ngModel)]="rfidOptions.checkpointAggregationWindowMs">
      </mat-form-field>
      <mat-form-field class="w-100">
          <input matInput placeholder="RPS Threshold"
                 type="number" [(ngModel)]="rfidOptions.rpsThreshold">
          <small>Read Per Second threshold to report poor tag reading</small>
      </mat-form-field>      
      <mat-slide-toggle [(ngModel)]="rfidOptions.enabled">Rfid enabled</mat-slide-toggle>
      <div class="row mt-2">
          <div class="col"></div>
          <div class="col-auto">
              <button type="submit" mat-raised-button color="primary" (click)="saveOptions()">Save</button>
          </div>
          <div class="col-auto">
              <button type="reset" mat-raised-button (click)="loadOptions()">Reset</button>
          </div>
      </div>
  `,
  styles: []
})
export class OptionsViewComponent implements OnInit {
  rfidOptions: RfidOptions = {};

  constructor(private http: HttpClient) {
    this.loadOptions().then();
  }

  async loadOptions() {
    this.rfidOptions = await this.http.get<RfidOptions>('options').toPromise();
  }

  async saveOptions() {
    await this.http.put('options', this.rfidOptions).toPromise();
    await this.loadOptions();
  }

  ngOnInit() {
  }

}
