import {inject, Injectable, signal} from '@angular/core';
import {ContractViewerApiClient} from '../../client';

@Injectable({
  providedIn: 'root'
})
export class AuthStore {
  private readonly client = inject(ContractViewerApiClient);

  constructor() {
    if (this.token()) {
      this.client.configuration.credentials = { Bearer: this.token()! };
    }
  }

  token() {
    return localStorage.getItem("auth_token")
  }

  setToken(token: string) {
    localStorage.setItem("auth_token", token)
    this.client.configuration.credentials = { Bearer: token };
  }
}
