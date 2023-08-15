import {ChangeDetectorRef, Component, OnDestroy} from '@angular/core';
import {MediaMatcher} from "@angular/cdk/layout";
import { OptionsService } from './service/options.service';
import {WebSocketConnectionService} from "@app/service/web-socket-connection-service";
import {TimingSessionsService} from "@app/service/timingSessionsService";
import {EventSelectorService} from "@app/service/event-selector-service";

@Component({
  selector: 'app-root',
  template: `
    <mat-toolbar color="primary" class="fixed-top navbar navbar-dark">
        <span class="d-flex flex-shrink-0">
            <a class="navbar-brand" routerLink="">BRAAAP</a>
        </span>
      <div class="flex-grow-1">
        <i class="material-icons" [class.text-success]="ws.isConnected"
           [class.text-danger]="!ws.isConnected"
           ngbTooltip="Соединение с сайтом">public</i>
        <i class="material-icons"
           *ngIf="ws.isConnected"
           [class.text-success]="(optionsService.$options|async)?.enabled"
           [class.text-danger]="!(optionsService.$options|async)?.enabled"
           ngbTooltip="RFID включен">network_check</i>
        <i class="material-icons"
           *ngIf="ws.isConnected && ts.activeSessionsCount > 0"
           [class.text-success]="ts.activeSessionsCount > 0"
           ngbTooltip="Идёт запись заездов {{ts.activeSessionsCount}}">receipt_long</i>
      </div>
      <mat-divider></mat-divider>
      <button mat-icon-button (click)="sidenav.toggle()"
              [hidden]="!mobileQuery.matches"><i class="material-icons">menu</i></button>
    </mat-toolbar>
    <mat-sidenav-container class="h-100">
      <mat-sidenav #sidenav [mode]="mobileQuery.matches ? 'over' : 'side'" [opened]="!mobileQuery.matches"
                   position="end"
                   [fixedInViewport]="mobileQuery.matches"
                   [fixedTopGap]="mobileQuery.matches ? 56 : 64"
                   (click)="mobileQuery.matches ? sidenav.toggle() : false">
        <mat-nav-list>
          <a mat-list-item routerLinkActive="text-danger" routerLink="/main">Main</a>
          <a mat-list-item routerLinkActive="text-danger" routerLink="/event-selector">Выбор гонки</a>
          <a *ngIf="ec.selectedEventId" mat-list-item routerLinkActive="text-danger"
             [routerLink]="['/event', ec.selectedEventId]">Заезды</a>
          <a mat-list-item routerLinkActive="text-danger" routerLink="/active-timings">Активные засечки</a>
          <a mat-list-item routerLinkActive="text-danger" routerLink="/options">Options</a>
          <mat-divider></mat-divider>
          <a mat-list-item href="/files" (click)="goto('/files')">File Browser</a>
          <a mat-list-item (click)="portainer('9000')">Portainer</a>
          <mat-divider></mat-divider>
          <a mat-list-item disabled>{{optionsService.version}}</a>
        </mat-nav-list>
      </mat-sidenav>

      <mat-sidenav-content>
        <div class="container-fluid d-flex flex-column h-100">
          <router-outlet></router-outlet>
        </div>
      </mat-sidenav-content>
    </mat-sidenav-container>
    <ngx-ui-loader></ngx-ui-loader>
  `,
  styles: []
})
export class AppComponent implements OnDestroy{
  public mobileQuery: MediaQueryList;
  private readonly _mobileQueryListener = () => {};


  constructor(changeDetectorRef: ChangeDetectorRef, media: MediaMatcher,
              public optionsService: OptionsService,
              public ts: TimingSessionsService,
              public ec: EventSelectorService,
              public ws: WebSocketConnectionService) {
    this.mobileQuery = media.matchMedia('(max-width: 600px)');
    this.mobileQuery.addEventListener("change", this._mobileQueryListener);
  }

  ngOnDestroy(): void {
    this.mobileQuery.removeEventListener("change", this._mobileQueryListener);
  }

  goto(s: string) {
    location.href=s;
  }

  portainer(port: string) {
    location.href = `${location.protocol}//${location.hostname}:${port}`;
  }
}
