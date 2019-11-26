import {Checkpoint} from "./checkpoint";
import * as moment from "moment";

export interface ILowRpsCheckpoint
{
  firstSeen: string;
  lastSeen: string;
  riderId: string;
  count: number;
  minRps: number;
  maxRps: number;
}

export class LowRpsCheckpoint implements ILowRpsCheckpoint
{
  firstSeen: string;
  lastSeen: string;
  riderId: string;
  count: number;
  minRps: number;
  maxRps: number;

  constructor(props: ILowRpsCheckpoint) {
    Object.assign(this, props)
  }

  append(cp: Checkpoint) {
    if (this.riderId !== cp.riderId) {
      console.error("Wrong rider id", this.riderId, cp.riderId);
      return
    }
    this.firstSeen = moment.min(moment(this.firstSeen), moment(cp.timestamp)).format();
    this.lastSeen = moment.max(moment(this.lastSeen), moment(cp.lastSeen)).format();
    this.count++;
    this.minRps = Math.min(this.minRps, cp.rps);
    this.maxRps = Math.max(this.maxRps, cp.rps);
  }

  static from(cp: Checkpoint): LowRpsCheckpoint {
    return new LowRpsCheckpoint({
      firstSeen: cp.timestamp,
      lastSeen: cp.lastSeen,
      riderId: cp.riderId,
      count: 1,
      minRps: cp.rps,
      maxRps: cp.rps
    });
  }
}
