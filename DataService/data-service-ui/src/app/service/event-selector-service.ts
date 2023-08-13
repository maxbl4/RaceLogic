import {Injectable} from "@angular/core";

const key = "EventSelectorService.selectedEventId"
@Injectable()
export class EventSelectorService {
  get selectedEventId():string|undefined {
    return localStorage.getItem(key) ?? undefined;
  }
  set selectedEventId(value: string|undefined) {
    if (!value)
      localStorage.removeItem(key);
    else
      localStorage.setItem(key, value);
  }
}
