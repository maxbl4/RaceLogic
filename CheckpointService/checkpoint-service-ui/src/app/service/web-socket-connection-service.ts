﻿import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {Checkpoint} from "../model/checkpoint";
import {ReaderStatus} from "../model/reader-status";
import * as moment from "moment";
import {BehaviorSubject, ReplaySubject} from "rxjs";
import {Injectable} from "@angular/core";
import {RfidOptions} from "../model/rfid-options";

@Injectable()
export class WebSocketConnectionService {
  private connection: HubConnection;
  readonly $checkpoints = new ReplaySubject<Checkpoint[]>();
  readonly $readerStatus = new BehaviorSubject<ReaderStatus>({isConnected: false, heartbeat: ''});
  readonly $rfidOptions = new BehaviorSubject<RfidOptions>({});
  readonly subscriptionStartTime = moment().utc().startOf('day')
    .subtract(5, 'days').format();

  constructor() {
    this.connection = new HubConnectionBuilder().withUrl("/_ws/cp").build();
    this.connection.on("Checkpoint", (cps:Checkpoint[]) => this.$checkpoints.next(cps));
    this.connection.on("ReaderStatus", (status:ReaderStatus) => {
      this.$readerStatus.next(status);
    });
    this.connection.on("RfidOptions", (rfidOptions:RfidOptions) => {
      this.$rfidOptions.next(rfidOptions);
    });
    this.connection.start().then(x => {
      console.log(`Subscribing to checkpoints at ${this.subscriptionStartTime}`);
      this.connection.send("Subscribe", this.subscriptionStartTime).then();
    });
  }
}
