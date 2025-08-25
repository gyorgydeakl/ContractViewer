import {inject, Injectable} from '@angular/core';
import {ContractViewerClient} from '../../client';

@Injectable({
  providedIn: 'root'
})
export class AuthStore {
  private readonly client = inject(ContractViewerClient);

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
