import {RfidOptions} from "../model/rfid-options";
import {HttpClient} from "@angular/common/http";
import {BehaviorSubject} from "rxjs";
import {Injectable} from "@angular/core";

@Injectable()
export class OptionsService {
  $options = new BehaviorSubject<RfidOptions>({});

  constructor(private http: HttpClient) {
    this.loadOptions();
  }

  loadOptions() {
    this.http.get<RfidOptions>('options').subscribe(o => this.$options.next(o));
  }

  saveOptions(rfidOptions: RfidOptions) {
    this.http.put('options', rfidOptions).subscribe(r => this.loadOptions());
  }
}
