import { Routes } from '@angular/router';
import { Order } from './pages/order/order';
import { Tracking } from './pages/tracking/tracking';

export const routes: Routes = [
  { path: '', component: Order, title: 'Awesome Pizza' },
  { path: 'orders/:code', component: Tracking, title: 'Il tuo ordine' },
];
