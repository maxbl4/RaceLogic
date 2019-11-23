export class ConsoleLogService
{
  private readonly oldError: OnErrorEventHandler;
  public errors: ErrorDescription[] = [];

  constructor() {
    this.oldError = window.onerror;
    window.onerror = this.errorHandler;
    window.onerror = (errorMsg, url, line) => console.log("err");
    console.log('setup error handler');
  }

  errorHandler(errorMsg, url, line) {
    console.log('observed error!');
    this.errors.push({
      errorMsg: errorMsg, url: url, line: line
    })
    if (this.oldError)
      return this.oldError(errorMsg, url, line);
    return false;
  }
}

export class ErrorDescription {
  errorMsg: any;
  url: string;
  line: number
}
