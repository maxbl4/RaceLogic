import {WebSocketConnectionService} from "./web-socket-connection-service";
import {BehaviorSubject, combineLatest, timer} from "rxjs";
import {Injectable} from "@angular/core";
import {map} from "rxjs/operators";
import {ReaderStatus} from "../model/reader-status";
import * as moment from "moment";
import {OptionsService} from "./options.service";

@Injectable()
export class ReaderStatusService {
  isConnected: boolean = false;
  timestamp: string = '';
  $status = new BehaviorSubject<ReaderStatus>({isConnected: false, heartbeat:moment().format()});
  statusIconClass = "";
  statusIcon = "";
  active = false;
  lastHeartbeat = "";

  private rotationIndex = 0;

  constructor(ws: WebSocketConnectionService, options: OptionsService) {
    ws.$readerStatus.subscribe(status => {
      this.lastHeartbeat = moment().utc().format();
    });
    combineLatest(timer(500, 500), ws.$readerStatus, options.$options)
      .pipe(map((args) => {
        return {time:args[0], status: args[1], rfidEnabled: args[2].enabled};
      }))
      .subscribe(s => {
        this.$status.next(s.status);
        if (!s.rfidEnabled) {
          this.active = false;
          return;
        }
        this.active = true;
        const lastSuccess = moment().diff(moment(this.lastHeartbeat), 'milliseconds');
        if (s.status.isConnected && lastSuccess < 2000) {
          this.statusIcon = "sync";
          this.statusIconClass = `rotate-${this.rotationIndex*45}`;
          this.rotationIndex++;
          if (this.rotationIndex > 7)
            this.rotationIndex = 0;
        }else
        {
          this.statusIcon = "sync_disabled";
          this.rotationIndex = 0;
          this.statusIconClass = "";
        }
      });
  }
}
