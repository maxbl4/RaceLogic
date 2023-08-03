import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {BehaviorSubject} from "rxjs";
import {Injectable} from "@angular/core";
import {TimingSessionUpdate} from "@app/service/data-service-client";

@Injectable()
export class WebSocketConnectionService {
  private connection: HubConnection;
  readonly $timingSessionUpdates = new BehaviorSubject<TimingSessionUpdate>(new TimingSessionUpdate());

  constructor() {
    this.connection = new HubConnectionBuilder().withUrl("/_ws/race").build();
    this.connection.on("TimingSessionUpdate", (ts:TimingSessionUpdate) => this.$timingSessionUpdates.next(ts));
    this.connection.start().then(_ => {});
  }
}
