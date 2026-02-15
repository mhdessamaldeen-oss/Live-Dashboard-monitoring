import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AlertsState } from './alerts.reducer';

export const selectAlertsState = createFeatureSelector<AlertsState>('alerts');

export const selectRecentAlerts = createSelector(
    selectAlertsState,
    (state) => state.alerts
);

export const selectAlertSummary = createSelector(
    selectAlertsState,
    (state) => state.summary
);

export const selectAlertsLoading = createSelector(
    selectAlertsState,
    (state) => state.loading
);

export const selectUnreadAlertsCount = createSelector(
    selectAlertsState,
    (state) => state.unreadCount
);

export const selectTotalCount = createSelector(
    selectAlertsState,
    (state) => state.totalCount
);
