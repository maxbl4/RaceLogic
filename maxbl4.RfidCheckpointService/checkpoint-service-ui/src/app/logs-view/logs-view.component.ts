import {Component, OnDestroy, OnInit} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {LogFileInfo} from "../model/LogFileInfo";

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
    <div class="row">
        <div class="col">
            <button mat-raised-button color="warn" (click)="clearLogs()">Clear logs</button>
        </div>
    </div>
      <div class="row">
          <div class="col">
              <table class="table">
                  <tr *ngFor="let f of logFiles">
                      <td>{{f.file}}</td>
                      <td>{{(f.size/1024).toFixed(2)}}kB</td>
                  </tr>
              </table>
          </div>
      </div>
  `,
  host: {'class': 'flex-container'},
  styles: []
})
export class LogsViewComponent implements OnInit, OnDestroy {
  logs = '';
  logLineCount = 20;
  logFilter = '';
  private interval: NodeJS.Timer;
  logFiles: LogFileInfo[];

  constructor(private http: HttpClient) {
    this.interval = setInterval(() => this.updateLogs(), 1000);
    this.loadLogInfos();
  }

  async updateLogs() {
    let requestUri = 'log?';
    if (this.logLineCount > 0) {
      requestUri += `lines=${this.logLineCount}&`
    }
    if (this.logFilter != null && this.logFilter.length > 0) {
      requestUri += `filter=${this.logFilter}&`
    }
    this.logs = (await this.http.get(requestUri, {responseType: "text"}).toPromise()).toString();
  }

  ngOnInit() {
  }

  ngOnDestroy(): void {
    clearInterval(this.interval);
  }

  clearLogs() {
    if (confirm('Clear logs?')) {
      this.http.delete<number>('log').subscribe(x => {
        alert(`Removed ${x} files`);
        this.loadLogInfos();
      });
    }
  }

  private loadLogInfos() {
    this.http.get<LogFileInfo[]>('log/info').subscribe(x => this.logFiles = x);
  }
}
