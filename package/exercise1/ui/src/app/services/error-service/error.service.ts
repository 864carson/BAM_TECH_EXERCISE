import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ErrorService {

  constructor() { }

  formatError(e: any): string {
    return `${e.error.responseCode}: ${e.error.message}`;
  }

  handleError(e: any, componentDescriptor: string): void {
    try {
      this.handleApiError(e, componentDescriptor);
    } catch {
      this.handleTypescriptError(e, componentDescriptor);
    }
  }

  handleApiError(e: any, componentDescriptor: string): void {
    const msg = this.formatError(e);
    console.error(`${componentDescriptor}: ${msg}`);
    alert(msg);
  }

  handleTypescriptError(e: any, componentDescriptor: string): void {
    console.error(`${componentDescriptor}: ${e}`);
    alert(e);
  }
}
