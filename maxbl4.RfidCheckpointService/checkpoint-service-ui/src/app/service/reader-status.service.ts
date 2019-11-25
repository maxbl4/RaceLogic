import {WebSocketConnectionService} from "./web-socket-connection-service";
import {combineLatest, timer} from "rxjs";
import {Injectable} from "@angular/core";

@Injectable()
export class ReaderStatusService {
  isConnected: boolean = false;
  timestamp: string = '';

  constructor(ws: WebSocketConnectionService) {
    // timer(1000, 1000)
    //   .pipe(() => combineLatest(ws.readerStatus, (t, s) => {
    //     return {time:t, status: s};
    //   }))
    //   .subscribe(console.log);
  }
}
