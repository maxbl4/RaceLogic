import {RfidOptions} from "../model/rfid-options";
import {HttpClient} from "@angular/common/http";
import {BehaviorSubject} from "rxjs";
import {Injectable} from "@angular/core";
import {WebSocketConnectionService} from "./web-socket-connection-service";

@Injectable()
export class OptionsService {
  $options = new BehaviorSubject<RfidOptions>({});
  version: string = "";

  constructor(private http: HttpClient) {
    //, ws: WebSocketConnectionService
    //ws.$rfidOptions.subscribe(x => this.$options.next(x));
    this.loadOptions();
    this.http.get('version', {responseType: 'text'}).subscribe(x => this.version = x);
  }

  loadOptions() {
    this.http.get<RfidOptions>('options').subscribe(o => this.$options.next(o));
  }

  saveOptions(rfidOptions: RfidOptions) {
    this.http.put('options', rfidOptions).subscribe(r => this.loadOptions());
  }
}
