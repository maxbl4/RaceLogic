import { Component, OnInit } from '@angular/core';
import {ActivatedRoute, Router} from "@angular/router";
import {DataClient, TimingSessionDto} from "@app/service/data-service-client";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";

@Component({
  selector: 'app-timing-session-view',
  template: `
    <h3>{{timingSessionDto.name}}</h3>
    <h4>Засечки</h4>
    <button mat-raised-button color="accent" [hidden]="timingSessionDto.isRunning" (click)="startTimingSession()">Старт</button>
    <button mat-raised-button color="accent" [hidden]="!timingSessionDto.isRunning" (click)="stopTimingSession()">Стоп</button>
  `,
  styles: [
  ]
})
export class TimingSessionViewComponent implements OnInit {
  private id: string = "";
  timingSessionDto: TimingSessionDto = new TimingSessionDto();

  constructor(private route:ActivatedRoute, private dataClient: DataClient, private router: Router, private modalService: NgbModal) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.id = params.get("timingSessionId")!;
      this.loadSession();
    });
  }

  loadSession(){
    this.dataClient.getTimingSession(this.id).subscribe(x => {
      this.timingSessionDto = x;
    });
  }

  startTimingSession() {
    this.dataClient.startTimingSession(this.id).subscribe(() => this.loadSession());
  }

  stopTimingSession() {
    this.dataClient.stopTimingSession(this.id).subscribe(() => this.loadSession());
  }
}
