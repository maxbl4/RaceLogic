import {Injectable} from "@angular/core";
import {WebSocketConnectionService} from "@app/service/web-socket-connection-service";
import {DataClient, TimingSessionDto} from "@app/service/data-service-client";

@Injectable()
export class TimingSessionsService {
  public activeSessionsCount: number = 0;
  public sessions:TimingSessionDto[] = [];

  constructor(private dataClient: DataClient, private ws: WebSocketConnectionService) {
    this.ws.$activeTimingSessionsUpdates
      .subscribe(x => {
        this.sessions = x.sessions ?? [];
        this.activeSessionsCount = x.sessions?.length ?? 0;
      });
    this.dataClient.listActiveTimingSessions()
      .subscribe(x =>{
        this.sessions = x ?? [];
        this.activeSessionsCount = x?.length ?? 0;
      });
  }
}
