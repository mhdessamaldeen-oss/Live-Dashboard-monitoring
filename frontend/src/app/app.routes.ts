import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { MainLayoutComponent } from './shared/components/main-layout/main-layout.component';

export const routes: Routes = [
    {
        path: 'auth',
        loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
    },
    {
        path: '',
        component: MainLayoutComponent,
        canActivate: [authGuard],
        children: [
            {
                path: '',
                loadChildren: () => import('./features/dashboard/dashboard.routes').then(m => m.DASHBOARD_ROUTES)
            },
            {
                path: 'servers',
                loadChildren: () => import('./features/servers/servers.routes').then(m => m.SERVERS_ROUTES)
            },
            {
                path: 'alerts',
                loadChildren: () => import('./features/alerts/alerts.routes').then(m => m.ALERTS_ROUTES)
            },
            {
                path: 'reports',
                loadChildren: () => import('./features/reports/reports.routes').then(m => m.REPORTS_ROUTES)
            },
            {
                path: 'jobs',
                loadChildren: () => import('./features/jobs/jobs.routes').then(m => m.JOBS_ROUTES)
            },
            {
                path: 'users',
                loadChildren: () => import('./features/users/users.routes').then(m => m.USERS_ROUTES)
            }
        ]
    },
    {
        path: '**',
        redirectTo: ''
    }
];
