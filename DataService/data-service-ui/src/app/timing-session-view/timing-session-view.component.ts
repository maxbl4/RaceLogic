import { Component, OnInit } from '@angular/core';
import {ActivatedRoute, Router} from "@angular/router";
import {
  Checkpoint,
  DataClient, Rider, RiderEventInfoDto, SessionDto,
  TimingSessionDto,
  TimingSessionUpdate
} from "@app/service/data-service-client";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {WebSocketConnectionService} from "@app/service/web-socket-connection-service";
import {DateTime, Duration} from "luxon";

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
    <button class="ml-2" mat-raised-button color="primary"
            (click)="refreshRating()">
      <i class="material-icons">refresh</i>Пересчитать
    </button>
        Начало: {{timingSessionDto.startTime?.toFormat("HH:MM:ss")}}
        Осталось: {{durationLeft}}
        Обновлено: {{update?.updated?.toFormat("HH:mm:ss")}}
    <form (submit)="appendRiderId()" *ngIf="timingSessionDto.isRunning">
      <div class="input-group mb-3">
        <input type="number" class="form-control hide-arrows"
               name="manualRiderId"
               [(ngModel)]="manualRiderId" placeholder="Номер гонщика" aria-label="Номер гонщика">
        <div class="input-group-append">
          <button class="btn btn-outline-secondary" type="submit"
                  [disabled]="!manualRiderId"><i class="material-icons">send</i></button>
        </div>
      </div>
    </form>
    <table class="table table-bordered" *ngIf="update && update.rating">
      <thead>
      <tr>
        <td>№</td>
        <td>Класс</td>
        <td>ФИО</td>
        <td>Финиш</td>
        <td>Круги</td>
        <td *ngFor="let l of laps;let i = index">Круг {{i + 1}}</td>
      </tr>
      </thead>
      <tr *ngFor="let r of update.rating">
        <td>{{r.rider?.number}}</td>
        <td>{{r.rider?.class?.name}}</td>
        <td>{{r.rider?.lastName}} {{r.rider?.firstName}}</td>
        <td>{{r.finished}}</td>
        <td>{{r.lapCount}}</td>
        <td *ngFor="let l of laps;let i = index">
          <ng-container *ngIf="r.laps && r.laps.length > i">
            {{r.laps[i].duration?.toFormat("hh:mm:ss")}}
          </ng-container>
        </td>
      </tr>
    </table>
    <h3>50 последних отметок</h3>
    <table class="table table-bordered" *ngIf="recentCheckpoints">
      <thead>
      <tr>
        <td>Время</td>
        <td>№</td>
        <td>ФИО</td>
      </tr>
      </thead>
      <tr *ngFor="let r of recentCheckpoints">
        <td>{{getRelativeTime(r.timestamp)}}</td>
        <td>{{getRider(r.riderId)?.number ?? r.riderId}}</td>
        <td>{{getRiderName(r.riderId) ?? "#" + r.riderId}}</td>
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
  timingSessionDto: TimingSessionDto = new TimingSessionDto();
  update?: TimingSessionUpdate;
  laps: any[] = [];
  sessionDto: SessionDto = new SessionDto();
  private riderMap?: Map<string, Rider>;
  public recentCheckpoints: Checkpoint[] | undefined;

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

      this.ws.$timingSessionUpdates.subscribe(x => {
        if (x?.timingSessionId != this.id)
          return;
        this.update = x;
        this.mapRiders(x.riders);
        this.sliceCheckpoints(x.resolvedCheckpoints);
        this.laps = new Array(x.maxLapCount);
      });
      this.dataClient.getTimingSessionRating(this.id).subscribe(x => {
        if (x?.timingSessionId != this.id)
          return;
        this.update = x;
        this.mapRiders(x.riders);
        this.sliceCheckpoints(x.resolvedCheckpoints);
        this.laps = new Array(x.maxLapCount);
      });
    });
  }

  private sliceCheckpoints(resolvedCheckpoints: Checkpoint[] | undefined){
    this.recentCheckpoints = resolvedCheckpoints?.reverse().slice(0, 50);
  }
  private mapRiders(riders: Rider[] | undefined) {
    if (!riders) return;
    this.riderMap = new Map<string, Rider>();
    for (let r of riders){
      if (r.id) this.riderMap.set(r.id, r);
    }
  }

  public getRider(id?:string){
    if (id && this.riderMap) {
      return this.riderMap.get(id);
    }
    return undefined;
  }

  public getRiderName(id?:string){
    const r = this.getRider(id);
    if (!r) return undefined;
    return `${r.lastName} ${r.firstName}`;
  }

  loadSession(){
    this.dataClient.getTimingSession(this.id)
      .subscribe(x => this.timingSessionDto = x);
    this.dataClient.getSession(this.sessionId)
      .subscribe(x => this.sessionDto = x);
  }

  get durationLeft(): string{
    if (this.timingSessionDto.startTime && this.sessionDto.finishCriteria?.duration) {
      const elapsed = DateTime.now().diff(this.timingSessionDto?.startTime);
      if (elapsed.as('seconds') > this.sessionDto.finishCriteria.duration.as('seconds')) {
        return "-" + elapsed.plus(this.sessionDto.finishCriteria.duration.negate()).toFormat("hh:mm:ss");
      }else
        return this.sessionDto.finishCriteria.duration.plus(elapsed.negate()).toFormat("hh:mm:ss");
    }
    return "00:00:00";
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

  refreshRating() {
    this.dataClient.getTimingSessionRating(this.id, true).subscribe();
  }

  getRelativeTime(timestamp: DateTime | undefined) {
    if (timestamp && this.timingSessionDto.startTime) {
      return timestamp.diff(this.timingSessionDto.startTime).toFormat("hh:mm:ss");
    }
    return "00:00:00";
  }
}
