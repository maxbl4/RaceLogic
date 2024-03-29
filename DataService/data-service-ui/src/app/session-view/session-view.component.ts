import { Component, OnInit } from '@angular/core';
import {ActivatedRoute, Router} from "@angular/router";
import {DataClient, SessionDto, TimingSessionDto} from "@app/service/data-service-client";
import {Observable} from "rxjs";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {TimingSessionAddDialogComponent} from "@app/timing-session-add-dialog/timing-session-add-dialog.component";

@Component({
  selector: 'app-session-view',
  template: `
    <a [routerLink]="['/event', eventId]">Назад к гонке</a>
    <h3>{{sessionDto.name}}</h3>
    <h4>Засечки</h4>
    <button mat-raised-button color="primary" (click)="addTimingSession()">Начать новую засечку</button>
    <table class="table table-bordered mt-3">
      <thead>
      <tr>
        <td>Название</td>
        <td>Активна</td>
        <td>Начало</td>
        <td>Конец</td>
        <td>_</td>
      </tr>
      </thead>
      <tbody>
      <tr *ngFor="let s of timingSessions|async" [class.table-primary]="s.isRunning">
        <td><a [routerLink]="['timing', s.id]">{{s.name}}</a></td>
        <td>{{s.isRunning ? "Да" : ""}}</td>
        <td>{{s.startTime?.toFormat("HH:mm")}}</td>
        <td>{{s.stopTime?.toFormat("HH:mm")}}</td>
        <td>
          <button mat-raised-button color="accent" (click)="deleteTimingSession(s)">Удалить</button>
        </td>
      </tr>
      </tbody>
    </table>
  `,
  styles: [
  ]
})
export class SessionViewComponent implements OnInit {
  public id: string = "";
  public eventId: string = "";
  sessionDto: SessionDto = new SessionDto();
  timingSessions: Observable<TimingSessionDto[]>|undefined;

  constructor(private route:ActivatedRoute, private dataClient: DataClient, private router: Router, private modalService: NgbModal) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.eventId = params.get("eventId")!;
      this.id = params.get("sessionId")!;
      this.dataClient.getSession(this.id).subscribe(x => {
        this.sessionDto = x;
        this.timingSessions = this.dataClient.listTimingSessions(this.id);
      });
    });
  }

  addTimingSession() {
    const modalRef = this.modalService.open(TimingSessionAddDialogComponent);
    modalRef.componentInstance.name = 'World';
    modalRef.componentInstance.session = this.sessionDto;
    modalRef.result.then((newTimingSession: TimingSessionDto) => {
      if (newTimingSession)
      {
        this.dataClient.startNewTimingSession(newTimingSession).subscribe(createdSessionId => {
          this.router.navigate(['timing', createdSessionId], {relativeTo: this.route});
        });
      }
    });
  }

  deleteTimingSession(session:SessionDto) {
    if (confirm(`Удалить заезд ${session.name}?`)) {
      this.dataClient.deleteTimingSession(session.id).subscribe(_ => this.timingSessions = this.dataClient.listTimingSessions(this.id));
    }
  }
}
