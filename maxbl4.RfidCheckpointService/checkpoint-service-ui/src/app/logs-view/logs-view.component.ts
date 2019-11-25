import { Component, OnInit } from '@angular/core';
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-logs-view',
  template: `
      <mat-form-field class="w-100">
          <input matInput placeholder="Display Log Count"
                 type="number" [(ngModel)]="logLineCount">
      </mat-form-field>
      <mat-form-field class="w-100">
          <input matInput placeholder="Log Filter"
                 [(ngModel)]="logFilter">
      </mat-form-field>
      <div class="row">
          <pre class="w-100 small" style="overflow-x: scroll">{{logs}}</pre>
      </div>
  `,
  host: {'class': 'flex-container'},
  styles: []
})
export class LogsViewComponent implements OnInit {
  logs = '';
  logLineCount = 20;
  logFilter = '';

  constructor(private http: HttpClient) {
    setInterval(() => this.updateLogs(), 1000);
  }

  async updateLogs() {
    let requestUri = 'log/';
    if (this.logLineCount > 0) {
      requestUri += `${this.logLineCount}/`
    }
    if (this.logFilter != null && this.logFilter.length > 0) {
      requestUri += `${this.logFilter}/`
    }
    this.logs = (await this.http.get(requestUri, {responseType: "text"}).toPromise()).toString();
  }

  ngOnInit() {
  }

}
