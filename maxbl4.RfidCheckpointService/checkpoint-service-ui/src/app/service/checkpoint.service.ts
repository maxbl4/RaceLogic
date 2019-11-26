import {Checkpoint} from "../model/checkpoint";
import * as moment from "moment";
import {WebSocketConnectionService} from "./web-socket-connection-service";
import {Injectable} from "@angular/core";
import {Observable} from "rxjs";

@Injectable()
export class CheckpointService {
  $checkpoints: Observable<Checkpoint[]>;
  checkpointsCount = 0;
  subscriptionTime = moment().utc().startOf('day').format();
  constructor(ws: WebSocketConnectionService) {
    this.$checkpoints = ws.$checkpoints;
    ws.$checkpoints.subscribe(cps => {
      this.checkpointsCount += cps.length;
    });
  }
}
