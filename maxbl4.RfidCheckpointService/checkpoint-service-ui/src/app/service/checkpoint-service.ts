import {HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {Checkpoint} from "../model/checkpoint";
import {ReaderStatus} from "../model/reader-status";

export class CheckpointService {
  private connection: HubConnection;
  constructor() {
    this.connection = new HubConnectionBuilder().withUrl("/ws/cp").build();
    this.connection.on("Checkpoint", (cp:Checkpoint) => console.log("checkpoint", cp));
    this.connection.on("ReaderStatus", (status:ReaderStatus) => console.log("ReaderStatus", status));
    this.connection.start().then(x => {
      this.connection.send("Subscribe", "2019-01-01").then();
    });
  }
}
