import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {BehaviorSubject} from "rxjs";
import {Injectable} from "@angular/core";
import {ActiveTimingSessionsUpdate, TimingSessionUpdate} from "@app/service/data-service-client";

@Injectable()
export class WebSocketConnectionService {
  private connection: HubConnection;
  readonly $timingSessionUpdates = new BehaviorSubject<TimingSessionUpdate>(new TimingSessionUpdate());
  readonly $activeTimingSessionsUpdates = new BehaviorSubject<ActiveTimingSessionsUpdate>(new ActiveTimingSessionsUpdate());

  constructor() {
    this.connection = new HubConnectionBuilder().withUrl("/_ws/race").build();

    this.connection.on("TimingSessionUpdate",
      (ts:TimingSessionUpdate) =>
      this.$timingSessionUpdates.next(TimingSessionUpdate.fromJS(ts)));
    this.connection.on("ActiveTimingSessionsUpdate",
      (ts:ActiveTimingSessionsUpdate) => {
        this.$activeTimingSessionsUpdates.next(ActiveTimingSessionsUpdate.fromJS(ts))
      });

    this.connection.start().then(_ => {});
  }
}
