import {Checkpoint} from "../model/checkpoint";
import * as moment from "moment";
import {WebSocketConnectionService} from "./web-socket-connection-service";
import {Injectable} from "@angular/core";
import {combineLatest, Observable} from "rxjs";
import {bufferTime, filter, map, mergeMap} from "rxjs/operators";
import {OptionsService} from "./options.service";

@Injectable()
export class CheckpointService {
  readonly $checkpoints: Observable<Checkpoint[]>;
  readonly $regularCheckpoints: Observable<Checkpoint[]>;
  readonly $aggregatedCheckpoints: Observable<Checkpoint[]>;
  readonly $lowRpscheckpoints: Observable<Checkpoint[]>;

  checkpointsCount = 0;
  subscriptionTime = moment().utc().startOf('day').format();

  constructor(ws: WebSocketConnectionService, optionsService: OptionsService) {
    this.$checkpoints = ws.$checkpoints;
    ws.$checkpoints.subscribe(cps => {
      this.checkpointsCount += cps.length;
    });

    this.$checkpoints = ws.$checkpoints.pipe(
      mergeMap(x => x),
      bufferTime(1000));

    this.$regularCheckpoints = ws.$checkpoints.pipe(
      mergeMap(x => x),
      filter(x => !x.aggregated),
      bufferTime(1000));

    this.$aggregatedCheckpoints = ws.$checkpoints.pipe(
      mergeMap(x => x),
      filter(x => x.aggregated),
      bufferTime(1000));

    this.$lowRpscheckpoints = combineLatest(optionsService.$options, ws.$checkpoints.pipe(mergeMap(x => x)))
      .pipe(
        filter(([options, cp]) => !cp.isManual && cp.aggregated && cp.rps < options.rpsThreshold),
        map(([options, cp]) => cp),
        bufferTime(1000));
  }
}
