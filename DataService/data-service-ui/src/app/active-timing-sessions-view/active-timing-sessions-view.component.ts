import { Component } from '@angular/core';
import {TimingSessionsService} from "@app/service/timingSessionsService";

@Component({
  selector: 'app-active-timing-sessions-view',
  template: `
    <h3>Активные засечки</h3>
    <table class="table table-bordered mt-3">
      <thead>
      <tr>
        <td>Название</td>
        <td>Начало</td>
        <td>Конец</td>
        <td>_</td>
      </tr>
      </thead>
      <tbody>
      <tr *ngFor="let s of ts.sessions">
        <td><a [routerLink]="['/event', s.eventId, 'session', s.sessionId, 'timing', s.id]">{{s.name}}</a></td>
        <td>{{s.startTime?.toFormat("HH:mm:ss")}}</td>
        <td>{{s.stopTime?.toFormat("HH:mm:ss")}}</td>
      </tr>
      </tbody>
    </table>
  `,
  styles: [
  ]
})
export class ActiveTimingSessionsViewComponent {
  constructor(public ts: TimingSessionsService) {
  }
}
