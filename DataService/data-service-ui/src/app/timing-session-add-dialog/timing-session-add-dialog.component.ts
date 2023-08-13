import {Component, Input, OnInit} from '@angular/core';
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";
import {SessionDto, TimingSessionDto} from "@app/service/data-service-client";

@Component({
  selector: 'app-timing-session-add-dialog',
  template: `
    <mat-card class="d-print-none">
      <mat-card-header>
        <mat-card-title>Записать заезд</mat-card-title>
      </mat-card-header>
      <mat-card-content>
        <mat-form-field class="w-100">
          <mat-label>Название</mat-label>
          <input matInput type="text" placeholder="" [(ngModel)]="timingSession.name">
        </mat-form-field>
        <mat-slide-toggle class="w-100" >Использовать RFID</mat-slide-toggle>
<!--        <mat-form-field class="w-100">-->
<!--          <mat-label>Дата бетонирования</mat-label>-->
<!--          <input matInput [matDatepicker]="picker1" >-->
<!--          <mat-datepicker-toggle matSuffix [for]="picker1"></mat-datepicker-toggle>-->
<!--          <mat-datepicker #picker1></mat-datepicker>-->
<!--        </mat-form-field>-->
        <br/>
        <br/>
        <button mat-raised-button color="primary" (click)="save()" [disabled]="!isFormValid">Дальше</button>
      </mat-card-content>
    </mat-card>
  `,
  styles: [
  ]
})
export class TimingSessionAddDialogComponent implements OnInit {

  private _session: SessionDto = new SessionDto();
  @Input()
  get session(): SessionDto { return this._session; }
  set session(value: SessionDto) {
    this._session = value;
    this.timingSession.sessionId = value.id;
    this.timingSession.name = `Заезд ${value.name}`;
  }
  timingSession: TimingSessionDto = new TimingSessionDto();

  get isFormValid(){
    return this.timingSession.name && this.timingSession.name.length > 0;
  }

  constructor(private activeModal: NgbActiveModal) { }

  ngOnInit(): void {
  }

  save() {
    this.activeModal.close(this.timingSession);
  }
}
