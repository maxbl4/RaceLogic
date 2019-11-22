import { Component } from '@angular/core';
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-root',
  template: `
    Checkpoint UI<br>
    <textarea rows="20" style="width: 100%;">{{logs}}</textarea>
  `,
  styles: []
})
export class AppComponent {
  title = 'checkpoint-service-ui';
  rfidOptions = '';
  logs = '';
  constructor(private http: HttpClient) {
    this.http.get<string>('rfid').subscribe(x => this.rfidOptions = JSON.stringify(x));
    setInterval(() => this.updateLogs(), 1000);
  }

  async updateLogs() {
    this.logs = (await this.http.get('log/20', {responseType: "text"}).toPromise()).toString();
  }
}
