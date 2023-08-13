import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {BehaviorSubject} from "rxjs";
import {Injectable} from "@angular/core";
import {ActiveTimingSessionsUpdate, RiderEventInfoUpdate, TimingSessionUpdate} from "@app/service/data-service-client";

@Injectable()
export class WebSocketConnectionService {
  public isConnected = false;
  private connection: HubConnection;
  readonly $timingSessionUpdates = new BehaviorSubject<TimingSessionUpdate>(new TimingSessionUpdate());
  readonly $activeTimingSessionsUpdates = new BehaviorSubject<ActiveTimingSessionsUpdate>(new ActiveTimingSessionsUpdate());
  readonly $riderEventInfoUpdate = new BehaviorSubject<RiderEventInfoUpdate>(new RiderEventInfoUpdate());

  constructor() {
    this.connection = new HubConnectionBuilder().withUrl("/_ws/race").build();
    this.connection.onclose(err => {
      this.isConnected = false;
      console.log("Ws Connection closed: ", err);
      setTimeout(() => this.startConnection(), 2000);
    });

    this.connection.on("TimingSessionUpdate",
      (x:TimingSessionUpdate) =>
      this.$timingSessionUpdates.next(TimingSessionUpdate.fromJS(x)));
    this.connection.on("ActiveTimingSessionsUpdate",
      (x:ActiveTimingSessionsUpdate) => {
        this.$activeTimingSessionsUpdates.next(ActiveTimingSessionsUpdate.fromJS(x))
      });
    this.connection.on("RiderEventInfoUpdate",
      (x:RiderEventInfoUpdate) => {
        this.$riderEventInfoUpdate.next(RiderEventInfoUpdate.fromJS(x))
      });
    this.startConnection();
  }

  startConnection(){
    this.connection.start()
      .then(_ => {
        this.isConnected = true;
        console.log("Ws Connected");
      })
      .catch(err => {
        console.log("Ws Connect failed: ", err);
        this.isConnected = false;
        setTimeout(() => this.startConnection(), 2000);
      });
  }
}
