import { createAction, props } from '@ngrx/store';
import { ServerDto } from '../models/server.models';
import { AlertSummaryDto } from '../models/alert.models';

export const loadDashboard = createAction('[Dashboard] Load Dashboard');
export const loadDashboardSuccess = createAction(
    '[Dashboard] Load Dashboard Success',
    props<{ serversCount: number; alerts: any[]; summary: AlertSummaryDto }>()
);
export const loadDashboardFailure = createAction(
    '[Dashboard] Load Dashboard Failure',
    props<{ error: string }>()
);

export const toggleTheme = createAction('[UI] Toggle Theme');
export const setTheme = createAction('[UI] Set Theme', props<{ theme: 'light' | 'dark' }>());
