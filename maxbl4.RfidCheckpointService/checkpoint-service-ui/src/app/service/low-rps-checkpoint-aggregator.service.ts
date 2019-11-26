import {OptionsService} from "./options.service";
import {BehaviorSubject, Observable, ReplaySubject, Subscription} from "rxjs";
import { RowDataTransaction } from '@ag-grid-community/all-modules';
import {LowRpsCheckpoint} from "../model/low-rps-checkpoint";
import {CheckpointService} from "./checkpoint.service";
import {Checkpoint} from "../model/checkpoint";
import {Injectable} from "@angular/core";

@Injectable()
export class LowRpsCheckpointAggregatorService {
  $updates = new BehaviorSubject<Observable<RowDataTransaction|null>|null>(null);
  readonly checkpoints: Map<string,LowRpsCheckpoint> = new Map<string, LowRpsCheckpoint>();
  rpsThreshold = 0;
  private subscription: Subscription;
  private $transactions: ReplaySubject<RowDataTransaction|null>;

  constructor(optionsService: OptionsService, private checkpointService: CheckpointService) {
    optionsService.$options.subscribe(x => this.initialize(x.rpsThreshold));
    console.log(`Create Low Rps Aggregator`);
  }

  initialize(rpsThreshold: number) {
    if (!rpsThreshold || this.rpsThreshold === rpsThreshold) return;
    this.rpsThreshold = rpsThreshold;
    console.log(`Initializing Low Rps Aggregator for ${rpsThreshold}`);
    if (this.subscription) this.subscription.unsubscribe();
    if (this.$transactions)
      this.$transactions.unsubscribe();
    this.$transactions = new ReplaySubject<RowDataTransaction|null>();
    this.$updates.next(this.$transactions);

    this.checkpoints.clear();
    this.subscription = this.checkpointService.$lowRpscheckpoints.subscribe(x => this.aggregate(x))
  }

  private aggregate(x: Checkpoint[]) {
    if (x.length < 1) return;
    const toAdd = new Set<LowRpsCheckpoint>();
    const toUpdate = new Set<LowRpsCheckpoint>();
    x.forEach(cp => {
      if (!this.checkpoints.has(cp.riderId)) {
        const agg = LowRpsCheckpoint.from(cp);
        this.checkpoints.set(cp.riderId, agg);
        toAdd.add(agg);
      }else{
        const agg = this.checkpoints.get(cp.riderId);
        agg.append(cp);
        toUpdate.add(agg);
      }
    });
    this.$transactions.next({add:Array.from(toAdd)});
    this.$transactions.next({update: Array.from(toUpdate)});
  }
}
