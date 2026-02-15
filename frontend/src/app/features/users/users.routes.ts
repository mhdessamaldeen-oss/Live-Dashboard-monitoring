import { Routes } from '@angular/router';
import { UserListComponent } from './user-list/user-list.component';

import { UsersManagement } from './users-management/users-management';

export const USERS_ROUTES: Routes = [
    {
        path: '',
        component: UsersManagement,
        children: [
            { path: '', component: UserListComponent },
            { path: 'roles', loadComponent: () => import('./role-list/role-list').then(m => m.RoleList) }
        ]
    }
];
