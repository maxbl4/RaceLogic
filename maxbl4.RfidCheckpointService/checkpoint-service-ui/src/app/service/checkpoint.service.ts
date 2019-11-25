import {Checkpoint} from "../model/checkpoint";
import * as moment from "moment";
import {WebSocketConnectionService} from "./web-socket-connection-service";
import {Injectable} from "@angular/core";
import {BehaviorSubject, combineLatest} from "rxjs";
import {OptionsService} from "./options.service";
import {filter, map} from "rxjs/operators";

@Injectable()
export class CheckpointService {
  $checkpoints = new BehaviorSubject<Checkpoint[]>([]);
  checkpoints: Checkpoint[] = [];
  subscriptionTime = moment().utc().startOf('day').format();
  constructor(ws: WebSocketConnectionService, private optionsService: OptionsService) {
    ws.checkpoints.subscribe(cp => {
      this.checkpoints.push(cp);
      this.$checkpoints.next(this.checkpoints.slice());
    });
  }

  getAggregateCheckpoints() {
    return this.$checkpoints
      .pipe(map(cps => cps.filter(cp => cp.aggregated)));
  }

  getLowRpsCheckpoints() {
    return combineLatest(this.optionsService.$options, this.$checkpoints)
      .pipe(map(([options, checkpoints]) => checkpoints.filter(cp => cp.aggregated && cp.rps < options.rpsThreshold)));
  }
}
