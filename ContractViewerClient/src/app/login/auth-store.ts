import {Injectable, signal} from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthStore {
  private readonly _token = signal<string | null>(null);
  readonly token = this._token.asReadonly();

  setToken(id: string) {
    this._token.set(id);
  }
}
