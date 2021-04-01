import { Component } from '@angular/core';
import {ChampionshipDto, ClassDto, DataClient, EventDto, SeriesDto} from "./service/data-service-client";

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
    <button class="btn btn-primary" (click)="purgeUpstreamData()">Purge upstream data</button>
    <button class="btn btn-primary" (click)="downloadUpstreamData()">Download upstream data</button>
    <span>Downloaded: {{downloadUpstreamDataStatus}}</span>
    <table class="table table-bordered">
      <ng-container *ngFor="let e of series">
        <tr>
          <td><button class="btn btn-sm btn-primary" (click)="expandSeries(e.id!)">{{expandedSeries.get(e.id!) == true ? "Collapse" : "Expand" }}</button></td>
          <td>{{e.id}}</td>
          <td>{{e.name}}</td>
          <td>{{e.description}}</td>
          <td>{{e.created}}</td>
          <td>{{e.updated}}</td>
        </tr>
        <tr *ngIf="expandedSeries.get(e.id!) == true">
          <td colspan="6">
            <table class="table table-bordered">
              <ng-container *ngFor="let c of championships.get(e.id!)">
                <tr>
                  <td>{{c.id}}</td>
                  <td>{{c.name}}</td>
                  <td>{{c.description}}</td>
                  <td>{{c.created}}</td>
                  <td>{{c.updated}}</td>
                </tr>
                <tr>
                  <td colspan="5">
                    <table class="table table-bordered">
                      <tr *ngFor="let cl of classes.get(c.id!)">
                        <td>{{cl.id}}</td>
                        <td>{{cl.name}}</td>
                        <td>{{cl.description}}</td>
                        <td>{{cl.created}}</td>
                        <td>{{cl.updated}}</td>
                      </tr>
                    </table>
                    <table class="table table-bordered">
                      <tr *ngFor="let e of events.get(c.id!)">
                        <td>{{e.id}}</td>
                        <td>{{e.date}}</td>
                        <td>{{e.name}}</td>
                        <td>{{e.description}}</td>
                        <td>{{e.basePrice}}</td>
                        <td>{{e.paymentMultiplier}}</td>
                        <td>{{e.startOfRegistration}}</td>
                        <td>{{e.endOfRegistration}}</td>
                        <td>{{e.created}}</td>
                        <td>{{e.updated}}</td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </ng-container>
            </table>
          </td>
        </tr>
      </ng-container>
    </table>
    <button class="btn btn-primary" (click)="addEvent()">Add</button>
    <router-outlet></router-outlet>
  `,
  styles: []
})
export class AppComponent {
  title = 'data-service-ui';
  downloadUpstreamDataStatus = "";
  series: SeriesDto[] = [];
  championships = new Map<string, ChampionshipDto[]>();
  classes = new Map<string, ClassDto[]>();
  events = new Map<string, EventDto[]>();
  expandedSeries = new Map<string, boolean>();
  constructor(private dataClient: DataClient) {
    this.loadSeries();
  }

  purgeUpstreamData() {
    this.dataClient.purgeUpstreamData().subscribe(e => {
      this.loadSeries();
    });
  }

  downloadUpstreamData() {
    this.dataClient.downloadUpstreamData(true).subscribe(e => {
      this.downloadUpstreamDataStatus = e.toString();
      this.loadSeries();
    });
  }

  loadSeries(){
    this.dataClient.listSeries().subscribe(x => this.series = x);
  }

  addEvent() {
    let ev = new EventDto();
    ev.name = "some";
    this.dataClient.upsertEvent('', ev).subscribe(x => {
    });
  }

  expandSeries(id: string) {
    let isExpanded = !(this.expandedSeries.get(id) == true);
    this.expandedSeries.set(id, isExpanded);
    if (isExpanded) {
      this.dataClient.listChampionships(id).subscribe(x => {
        this.championships.set(id, x);
        for (let c of x){
          this.dataClient.listClasses(c.id).subscribe(e => {
            console.log(e);
            this.classes.set(c.id!, e);
          });
        }
        for (let c of x){
          this.dataClient.listEvents(c.id).subscribe(e => this.events.set(c.id!, e));
        }
      });
    }
  }
}
