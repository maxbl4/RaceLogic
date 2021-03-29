import { Component } from '@angular/core';
import { v4 as uuidv4 } from 'uuid';
import * as moment from 'moment';
import {DataClient, EventDto} from "./service/data-service-client";

@Component({
  selector: 'app-root',
  template: `
    <div style="text-align:center" class="content">
      <h1>
        Welcome to {{title}}!
      </h1>
      <span style="display: block">{{ title }} app is running!</span>
      <img width="300" alt="Angular Logo" src="data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNTAgMjUwIj4KICAgIDxwYXRoIGZpbGw9IiNERDAwMzEiIGQ9Ik0xMjUgMzBMMzEuOSA2My4ybDE0LjIgMTIzLjFMMTI1IDIzMGw3OC45LTQzLjcgMTQuMi0xMjMuMXoiIC8+CiAgICA8cGF0aCBmaWxsPSIjQzMwMDJGIiBkPSJNMTI1IDMwdjIyLjItLjFWMjMwbDc4LjktNDMuNyAxNC4yLTEyMy4xTDEyNSAzMHoiIC8+CiAgICA8cGF0aCAgZmlsbD0iI0ZGRkZGRiIgZD0iTTEyNSA1Mi4xTDY2LjggMTgyLjZoMjEuN2wxMS43LTI5LjJoNDkuNGwxMS43IDI5LjJIMTgzTDEyNSA1Mi4xem0xNyA4My4zaC0zNGwxNy00MC45IDE3IDQwLjl6IiAvPgogIDwvc3ZnPg==">
    </div>
    <table class="table table-bordered">
      <tr>
        <td>Id</td>
        <td>Date</td>
        <td>Name</td>
        <td>ChampId</td>
        <td>StartReg</td>
        <td>X</td>
      </tr>
      <tr *ngFor="let e of events">
        <td>{{e.id}}</td>
        <td>{{e.date}}</td>
        <td>{{e.name}}</td>
        <td>{{e.championshipId}}</td>
        <td>{{e.startOfRegistration}}</td>
        <td>
          <button class="btn btn-primary" (click)="deleteEvent(e.id)">Delete</button></td>
      </tr>
    </table>
    <button class="btn btn-primary" (click)="addEvent()">Add</button>
    <router-outlet></router-outlet>
  `,
  styles: []
})
export class AppComponent {
  title = 'data-service-ui';
  events: EventDto[] = [];
  constructor(private dataClient: DataClient) {
    this.loadEvents();
  }

  loadEvents() {
    this.dataClient.listEvents().subscribe(e => {
      this.events = e;
      console.log(e);
    });
  }

  addEvent() {
    let ev = new EventDto();
    ev.name = "some";
    ev.championshipId = uuidv4();
    ev.startOfRegistration = moment();
    this.dataClient.upsertEvent(ev).subscribe(x => {
      this.loadEvents();
    });
  }

  deleteEvent(id:string) {
    this.dataClient.deleteEvent(id).subscribe(() => this.loadEvents());
  }
}
