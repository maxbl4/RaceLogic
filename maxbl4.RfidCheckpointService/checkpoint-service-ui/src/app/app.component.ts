import { Component } from '@angular/core';
import {HttpClient} from "@angular/common/http";

@Component({
  selector: 'app-root',
  template: `
    Checkpoint UI<br>
    {{rfidOptions}}
  `,
  styles: []
})
export class AppComponent {
  title = 'checkpoint-service-ui';
  rfidOptions = '';
  constructor(http: HttpClient) {
    http.get<string>('rfid').subscribe(x => this.rfidOptions = JSON.stringify(x));
  }
}
