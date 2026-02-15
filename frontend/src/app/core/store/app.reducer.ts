import { createReducer, on } from '@ngrx/store';
import * as AppActions from './app.actions';
import { AlertSummaryDto } from '../models/alert.models';

export interface AppState {
    theme: 'light' | 'dark';
    dashboard: {
        serversCount: number;
        activeAlerts: any[];
        summary: AlertSummaryDto | null;
        loading: boolean;
        error: string | null;
    };
}

export const initialState: AppState = {
    theme: 'light',
    dashboard: {
        serversCount: 0,
        activeAlerts: [],
        summary: null,
        loading: false,
        error: null
    }
};

export const appReducer = createReducer(
    initialState,
    on(AppActions.toggleTheme, (state) => ({
        ...state,
        theme: state.theme === 'light' ? 'dark' : 'light'
    })),
    on(AppActions.setTheme, (state, { theme }) => ({
        ...state,
        theme
    })),
    on(AppActions.loadDashboard, (state) => ({
        ...state,
        dashboard: { ...state.dashboard, loading: true, error: null }
    })),
    on(AppActions.loadDashboardSuccess, (state, { serversCount, alerts, summary }) => ({
        ...state,
        dashboard: {
            ...state.dashboard,
            serversCount,
            activeAlerts: alerts,
            summary,
            loading: false
        }
    })),
    on(AppActions.loadDashboardFailure, (state, { error }) => ({
        ...state,
        dashboard: { ...state.dashboard, loading: false, error }
    }))
);
