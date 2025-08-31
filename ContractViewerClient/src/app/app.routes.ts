import { Routes } from '@angular/router';
import {Login} from './login/login';
import {ContractList} from './contract-list';
import {PoaList} from './poa-list';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'contracts', component: ContractList },
  { path: 'login', component: Login },
  { path: 'poas', component: PoaList },
];
