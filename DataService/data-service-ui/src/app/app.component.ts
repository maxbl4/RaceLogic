import {ChangeDetectorRef, Component, OnDestroy} from '@angular/core';
import {MediaMatcher} from "@angular/cdk/layout";

@Component({
  selector: 'app-root',
  template: `
    <mat-toolbar color="primary" class="fixed-top navbar navbar-dark">
        <span class="d-flex flex-shrink-0">
            <a class="navbar-brand" routerLink="">BRAAAP</a>
        </span>
      <mat-divider></mat-divider>
      <button mat-icon-button (click)="sidenav.toggle()"
              [hidden]="!mobileQuery.matches"><i class="material-icons">menu</i></button>
    </mat-toolbar>
    <mat-sidenav-container class="h-100">
      <mat-sidenav #sidenav [mode]="mobileQuery.matches ? 'over' : 'side'" [opened]="!mobileQuery.matches" position="end"
                   [fixedInViewport]="mobileQuery.matches"
                   [fixedTopGap]="mobileQuery.matches ? 56 : 64"
                   (click)="mobileQuery.matches ? sidenav.toggle() : false">
        <mat-nav-list>
          <a mat-list-item routerLinkActive="text-danger" routerLink="/main">Main</a>
          <a mat-list-item routerLinkActive="text-danger" routerLink="/main">Main 2</a>
          <mat-divider></mat-divider>
          <a mat-list-item href="/files" (click)="goto('/files')">File Browser</a>
          <a mat-list-item (click)="portainer('9000')">Portainer</a>
          <mat-divider></mat-divider>
          <a mat-list-item disabled>1.2.3.4</a>
        </mat-nav-list>
      </mat-sidenav>

      <mat-sidenav-content>
        <div class="container-fluid d-flex flex-column h-100">
          <router-outlet></router-outlet>
        </div>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: []
})
export class AppComponent implements OnDestroy {
  public mobileQuery: MediaQueryList;
  private readonly _mobileQueryListener = () => {};


  constructor(changeDetectorRef: ChangeDetectorRef, media: MediaMatcher) {
    this.mobileQuery = media.matchMedia('(max-width: 600px)');
    this.mobileQuery.addListener(this._mobileQueryListener);
  }

  ngOnDestroy(): void {
    this.mobileQuery.removeListener(this._mobileQueryListener);
  }

  goto(s: string) {
    location.href=s;
  }

  portainer(port: string) {
    location.href = `${location.protocol}//${location.hostname}:${port}`;
  }
}
