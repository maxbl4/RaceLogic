import { Component, OnInit } from '@angular/core';
import {ActivatedRoute, Router} from "@angular/router";
import {
  DataClient, IRiderEventInfoDto,
  IRoundPosition,
  RiderEventInfoDto,
  TimingSessionDto,
  TimingSessionUpdate
} from "@app/service/data-service-client";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {WebSocketConnectionService} from "@app/service/web-socket-connection-service";
import {switchMap} from "rxjs";

@Component({
  selector: 'app-timing-session-view',
  template: `
    <a [routerLink]="['/event', eventId, 'session', sessionId]">Назад к заезду</a>
    <h3>{{timingSessionDto.name}}</h3>
    <button mat-raised-button color="accent" [hidden]="timingSessionDto.isRunning" (click)="resumeTimingSession()">
      Старт
    </button>
    <button mat-raised-button color="accent" [hidden]="!timingSessionDto.isRunning" (click)="stopTimingSession()">Стоп
    </button>
    <form (submit)="appendRiderId()">
      <div class="input-group mb-3">
        <input type="number" class="form-control hide-arrows"
               name="manualRiderId"
               [(ngModel)]="manualRiderId" placeholder="Номер гонщика" aria-label="Номер гонщика">
        <div class="input-group-append">
          <button class="btn btn-outline-secondary" type="submit"
            [disabled]="!manualRiderId">++</button>
        </div>
      </div>
    </form>
    <table class="table table-bordered" *ngIf="update && update.rating">
      <thead>
      <tr>
        <td>№</td>
        <td>ФИО</td>
        <td>Круги</td>
        <td *ngFor="let l of laps;let i = index">Круг {{i+1}}</td>
      </tr>
      </thead>
      <tr *ngFor="let r of rating">
        <td>{{r.rider?.number}}</td>
        <td>{{r.rider?.lastName}} {{r.rider?.firstName}}</td>
        <td>{{r.lapCount}}</td>
        <td *ngFor="let l of laps;let i = index">
          <ng-container *ngIf="r.laps && r.laps.length > i">
            {{r.laps[i].duration?.toFormat("hh:mm:ss")}}
          </ng-container>
        </td>
      </tr>
    </table>
  `,
  styles: [
  ]
})
export class TimingSessionViewComponent implements OnInit {
  public id: string = "";
  public sessionId: string = "";
  public eventId: string = "";
  public manualRiderId = "";
  private riders?: Map<string, RiderEventInfoDto>;
  timingSessionDto: TimingSessionDto = new TimingSessionDto();
  update?: TimingSessionUpdate;
  rating: IRoundPositionWithRider[] = [];
  laps: any[] = [];

  constructor(private route:ActivatedRoute, private dataClient: DataClient,
              private router: Router,
              private modalService: NgbModal,
              public ws: WebSocketConnectionService) {
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.id = params.get("timingSessionId")!;
      this.sessionId = params.get("sessionId")!;

      this.loadSession();

      this.dataClient.listRiderEventInfo(this.id)
        .pipe(switchMap(x => {
          this.mapRiders(x);
          return this.dataClient.getTimingSessionRating(this.id);
        }))
        .subscribe(x => {
          this.update = x;
          this.joinRiders(x);
          this.ws.$timingSessionUpdates.subscribe(x => {
            if (x?.timingSessionId != this.id)
              return;
            this.update = x;
            this.joinRiders(x);
          });
        });
    });
  }

  private joinRiders(x?: TimingSessionUpdate) {
    if (!x){
      this.rating = [];
      return;
    }
    this.rating = <IRoundPositionWithRider[]>x.rating ?? [];
    let maxLaps = 0;
    for (let r of this.rating) {
      r.rider = this.getRider(r.riderId);
      if (maxLaps < (r.laps?.length ?? 0)) {
        maxLaps = r.laps!.length;
      }
    }
    this.laps = new Array(maxLaps);
  }

  loadSession(){
    this.dataClient.getTimingSession(this.id)
      .subscribe(x => this.timingSessionDto = x);
  }

  mapRiders(riderInfos: RiderEventInfoDto[]){
    let map = new Map<string, RiderEventInfoDto>();
    for (let r of riderInfos){
      if (r.id) {
        map.set(r.id, r);
      }
    }
    this.riders = map;
  }

  getRider(id?:string):RiderEventInfoDto{
    if (id) {
      return this.riders?.get(id) ?? new RiderEventInfoDto({lastName: id});
    } else {
      return new RiderEventInfoDto();
    }
  }

  resumeTimingSession() {
    this.dataClient.resumeTimingSession(this.id).subscribe(() => this.loadSession());
  }

  stopTimingSession() {
    this.dataClient.stopTimingSession(this.id).subscribe(() => this.loadSession());
  }

  appendRiderId() {
    this.dataClient.manualCheckpoint(this.manualRiderId).subscribe()
    this.manualRiderId = "";
  }
}

interface IRoundPositionWithRider extends IRoundPosition{
  rider?: IRiderEventInfoDto;
}
