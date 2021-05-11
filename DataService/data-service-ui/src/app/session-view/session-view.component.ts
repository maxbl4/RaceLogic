import { Component, OnInit } from '@angular/core';
import {ActivatedRoute, Router} from "@angular/router";
import {DataClient, SessionDto, TimingSessionDto} from "../service/data-service-client";
import {Observable} from "rxjs";

@Component({
  selector: 'app-session-view',
  template: `
    <h3>{{sessionDto.name}}</h3>
    <h4>Засечки</h4>
    <button class="btn btn-primary" (click)="startTimingSession()">Начать новую засечку</button>
    <table class="table table-bordered">
      <thead>
      <tr>
        <td>Начало</td>
        <td>Конец</td>
        <td>Кол-во меток</td>
      </tr>
      </thead>
      <tbody>
      <tr *ngFor="let s of timingSessions|async">
        <td><a [routerLink]="['session', s.id]">{{s.name}}</a></td>
        <td>{{s.startTime?.toFormat("HH:mm")}}</td>
        <td>{{s.stopTime?.toFormat("HH:mm")}}</td>
      </tr>
      </tbody>
    </table>
  `,
  styles: [
  ]
})
export class SessionViewComponent implements OnInit {
  private id: string = "";
  sessionDto: SessionDto = new SessionDto();
  timingSessions: Observable<TimingSessionDto[]>|undefined;

  constructor(private route:ActivatedRoute, private dataClient: DataClient, private router: Router) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.id = params.get("sessionId")!;
      this.dataClient.getSession(this.id).subscribe(x => {
        this.sessionDto = x;
        this.timingSessions = this.dataClient.listTimingSessions(x.eventId);
      });
    });
  }

  startTimingSession() {
    this.dataClient.startTimingSession(this.id).subscribe(x => {
      this.router.navigate(['timing', x]);
    });
  }
}
