import { Routes } from '@angular/router';

export const ALERTS_ROUTES: Routes = [
    { path: '', loadComponent: () => import('./alert-list/alert-list.component').then(c => c.AlertListComponent) }
];
