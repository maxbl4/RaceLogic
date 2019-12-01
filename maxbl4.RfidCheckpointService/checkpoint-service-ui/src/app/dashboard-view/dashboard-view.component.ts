import { Component, OnInit } from '@angular/core';
import {CheckpointService} from "../service/checkpoint.service";
import {LowRpsCheckpointAggregatorService} from "../service/low-rps-checkpoint-aggregator.service";
import {OptionsService} from "../service/options.service";
import {RfidOptions} from "../model/rfid-options";
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-dashboard-view',
  template: `
    <div class="row">
        <div class="col-md mb-2">
            <div class="card h-100">
                <div class="card-body">
                    <h6 class="card-title">Enable RFID</h6>
                    <mat-slide-toggle [(ngModel)]="rfidEnabled">Rfid enabled</mat-slide-toggle>
                </div>
            </div>
        </div>
        <div class="col-md mb-2">
            <div class="card h-100">
                <div class="card-body">
                    <h6 class="card-title">Checkpoints</h6>
                    <h5>{{checkpointService.checkpointsCount}}</h5>
                    <button mat-raised-button routerLink="/monitor">Details</button>
                </div>
            </div>
        </div>
        <div class="col-md mb-2">
            <div class="card h-100">
                <div class="card-body">
                    <h6 class="card-title">Low RPS</h6>
                    <h5>{{lowRpsService.lowRpsCount}}</h5>
                    <button mat-raised-button routerLink="/low-rps">Details</button>
                </div>
            </div>
        </div>
        <div class="col-md mb-2">
            <div class="card h-100">
                <div class="card-body">
                    <h6 class="card-title">Manual input</h6>
                    <mat-form-field class="w-100">
                        <input matInput placeholder="Enter rider id and press Enter"
                               [(ngModel)]="manualRiderId" (keyup)="onManualRiderKey($event)">
                    </mat-form-field>
                </div>
            </div>
        </div>
    </div>
    <div class="row">
        <div class="col-md mb-2">
            <div class="card h-100">
                <div class="card-body">
                    <h6 class="card-title">Restart</h6>
                    <div>This will force backend to STOP. And it SHOULD be restarted by Docker in 10-20 seconds. Reboot device if backend does not come online</div>
                    <button mat-raised-button (click)="restart()" color="warn">Restart Backend</button>
                </div>
            </div>
        </div>
    </div>
  `,
  host: {'class': 'flex-container'},
  styles: []
})
export class DashboardViewComponent implements OnInit {
  options: RfidOptions = { enabled: false };
  manualRiderId: string;

  constructor(public checkpointService: CheckpointService,
              public lowRpsService: LowRpsCheckpointAggregatorService, private optionsService: OptionsService,
              private http: HttpClient) {
  }

  ngOnInit(): void {
    this.optionsService.$options.subscribe(x => this.options = x);
  }

  get rfidEnabled() {
    return this.options.enabled;
  }

  set rfidEnabled(v: boolean) {
    this.options.enabled = v;
    this.optionsService.saveOptions(this.options);
  }

  onManualRiderKey($event: KeyboardEvent) {
    if ($event.key == 'Enter') {
      this.checkpointService.sendManualCheckpoint(this.manualRiderId);
      this.manualRiderId = '';
    }
  }

  restart() {
    if (confirm('Restart?')) {
      this.http.post('restart', {}).subscribe();
    }
  }
}
