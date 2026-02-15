import { Routes } from '@angular/router';

export const SERVERS_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () => import('./server-list/server-list.component').then(m => m.ServerListComponent)
    },
    {
        path: ':id',
        loadComponent: () => import('./server-details/server-details.component').then(m => m.ServerDetailsComponent)
    }
];
