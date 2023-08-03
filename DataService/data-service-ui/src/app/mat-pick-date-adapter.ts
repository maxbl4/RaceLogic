import {Injectable} from "@angular/core";
import {NativeDateAdapter} from "@angular/material/core";
import {DateTime} from "luxon";

export const dateFormat = "dd.LL.yyyy";

export const PICK_FORMATS = {
  parse: {dateInput: {month: 'short', year: 'numeric', day: 'numeric'}},
  display: {
    dateInput: 'input',
    monthYearLabel: {year: 'numeric', month: 'short'},
    dateA11yLabel: {year: 'numeric', month: 'long', day: 'numeric'},
    monthYearA11yLabel: {year: 'numeric', month: 'long'}
  }
};

@Injectable()
export class PickDateAdapter extends NativeDateAdapter {
  override format(date: Date, displayFormat: Object): string {
    return DateTime.fromJSDate(date).toFormat(dateFormat);
  }
  override parse(value: any): Date | null {
    if (!value.toString().match(/^\d{2}\.\d{2}\.\d{4}$/))
      return null;
    return DateTime.fromFormat(value, dateFormat).toJSDate();
  }
}
