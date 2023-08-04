import { Component, OnInit } from '@angular/core';
import {RfidOptions} from "../model/rfid-options";
import {OptionsService} from "../service/options.service";

@Component({
  selector: 'app-options-view',
  template: `
      <mat-form-field class="w-100">
          <input matInput placeholder="Rfid Connection String" [(ngModel)]="rfidOptions.connectionString">
        <mat-hint>Local serial connection: Protocol=Serial;Serial=/dev/ttyS2@115200;RFPower=29;AntennaConfiguration=Antenna1</mat-hint>
      </mat-form-field>
      <mat-form-field class="w-100">
          <input matInput placeholder="Checkpoint aggregation window milliseconds"
                 type="number" [(ngModel)]="rfidOptions.checkpointAggregationWindowMs">
      </mat-form-field>
      <mat-form-field class="w-100">
          <input matInput placeholder="RPS Threshold"
                 type="number" [(ngModel)]="rfidOptions.rpsThreshold">
          <mat-hint>Read Per Second threshold to report poor tag reading</mat-hint>
      </mat-form-field>
      <mat-slide-toggle class="mt-2" [(ngModel)]="rfidOptions.persistTags">Persist raw tags</mat-slide-toggle>
      <mat-slide-toggle class="mt-2" [(ngModel)]="rfidOptions.enabled">Rfid enabled</mat-slide-toggle>
      <div class="row mt-2">
          <div class="col"></div>
          <div class="col-auto">
              <button type="submit" mat-raised-button color="primary" (click)="saveOptions()">Save</button>
          </div>
          <div class="col-auto">
              <button type="reset" mat-raised-button (click)="optionsService.loadOptions()">Reset</button>
          </div>
      </div>
  `,
  host: {'class': 'flex-container'},
  styles: []
})
export class OptionsViewComponent implements OnInit {
  rfidOptions: RfidOptions = {};

  constructor(public optionsService: OptionsService) { }

  saveOptions() {
    this.optionsService.saveOptions(this.rfidOptions);
  }

  ngOnInit() {
    this.optionsService.$options.subscribe(o => this.rfidOptions = Object.assign({}, o));
  }
}
