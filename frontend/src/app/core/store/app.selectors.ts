import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AppState } from './app.reducer';

export const selectAppState = createFeatureSelector<AppState>('app');

export const selectTheme = createSelector(
    selectAppState,
    (state) => state.theme
);

export const selectDashboard = createSelector(
    selectAppState,
    (state) => state.dashboard
);

export const selectDashboardLoading = createSelector(
    selectDashboard,
    (state) => state.loading
);

export const selectDashboardStats = createSelector(
    selectDashboard,
    (state) => {
        const summary = state.summary;
        const health = summary ? Math.max(100 - (summary.criticalAlerts * 5) - (summary.warningAlerts * 2), 0) : 100;

        return [
            { label: 'Total Servers', value: state.serversCount.toString(), icon: 'dns' },
            { label: 'Active Alerts', value: summary?.totalAlerts.toString() || '0', icon: 'warning' },
            { label: 'System Health', value: `${health}%`, icon: 'favorite' },
            { label: 'Uptime', value: '100%', icon: 'timer' }
        ];
    }
);

export const selectConnectionStatus = createSelector(
    selectDashboard,
    (state) => state.connectionStatus
);
