import {Checkpoint} from "../model/checkpoint";
import * as moment from "moment";
import {ReaderStatus} from "../model/reader-status";
import {WebSocketConnectionService} from "./web-socket-connection-service";
import {Injectable} from "@angular/core";

@Injectable()
export class CheckpointService {
  checkpoints: Checkpoint[] = [];
  subscriptionTime = moment().utc().startOf('day').format();
  readerStatus: ReaderStatus;
  constructor(ws: WebSocketConnectionService) {
    ws.checkpoints.subscribe(cp => this.checkpoints.push(cp));
  }

  getLowRpsCheckpoints() {

  }
}
