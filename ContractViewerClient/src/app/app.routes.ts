import { Routes } from '@angular/router';
import {ContractList} from './contract-list/contract-list';
import {Login} from './login/login';

export const routes: Routes = [
  { path: 'contracts', component: ContractList },
  { path: 'login', component: Login },
];
