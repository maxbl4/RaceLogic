import { Component, OnInit } from '@angular/core';
import * as moment from "moment";
import {WebSocketConnectionService} from "../service/web-socket-connection-service";
import {Sort} from "@angular/material/sort";
import {Checkpoint} from "../model/checkpoint";
import {CheckpointService} from "../service/checkpoint.service";

@Component({
  selector: 'app-monitor-view',
  template: `
    <mat-slide-toggle [(ngModel)]="showAggregated">Show aggregated</mat-slide-toggle>
    <table class="table">
        <thead>
            <tr>
                <th>Id</th>
                <th>Time</th>
                <th>RiderId</th>
                <th [hidden]="!showAggregated">Count</th>
            </tr>
        </thead>
        <tbody>
          <tr *ngFor="let cp of getCheckpoints()" [class.bg-info]="cp.aggregated">
              <td>{{cp.id}}</td>
              <td>{{moment(cp.timestamp).format('hh:mm:ss')}}</td>
              <td>{{cp.riderId}}</td>
              <td [hidden]="!showAggregated">{{cp.count}}</td>
          </tr>
        </tbody>
    </table>
  `,
  styles: []
})
export class MonitorViewComponent implements OnInit {
  moment = moment;
  showAggregated = false;

  constructor(public checkpointService: CheckpointService) {
  }

  getCheckpoints() {
    if (!this.showAggregated)
      return this.checkpointService.checkpoints.filter(x => !x.aggregated);
    return this.checkpointService.checkpoints;
  }

  ngOnInit() {
  }
}
