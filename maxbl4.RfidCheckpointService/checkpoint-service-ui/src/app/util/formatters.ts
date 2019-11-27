import * as moment from "moment";

export function formatDate(date: any): string {
  if (!date) return '';
  const d = moment(date);
  if (d.isSame(new Date(), "day"))
    return d.format('HH:mm:ss');
  return d.format();
}
