import { createReducer, on } from '@ngrx/store';
import * as AlertsActions from './alerts.actions';
import { AlertDto, AlertSummaryDto, AlertStatus } from '../../../core/models/alert.models';

export interface AlertsState {
    alerts: AlertDto[];
    totalCount: number;
    summary: AlertSummaryDto | null;
    loading: boolean;
    error: string | null;
    unreadCount: number;
}

export const initialAlertsState: AlertsState = {
    alerts: [],
    totalCount: 0,
    summary: null,
    loading: false,
    error: null,
    unreadCount: 0
};

export const alertsReducer = createReducer(
    initialAlertsState,
    on(AlertsActions.loadAlerts, (state) => ({
        ...state,
        loading: true,
        error: null
    })),
    on(AlertsActions.loadAlertsSuccess, (state, { result }) => ({
        ...state,
        alerts: result.items,
        totalCount: result.totalCount,
        loading: false
    })),
    on(AlertsActions.loadAlertsFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    })),
    on(AlertsActions.addAlert, (state, { alert }) => ({
        ...state,
        alerts: [alert, ...state.alerts].slice(0, 50), // Keep last 50
        unreadCount: state.unreadCount + 1,
        summary: state.summary ? {
            ...state.summary,
            totalAlerts: state.summary.totalAlerts + 1,
            activeAlerts: state.summary.activeAlerts + 1,
            // (we could increment severity specific counts if needed)
        } : null
    })),
    on(AlertsActions.resolveAlertSuccess, (state, { id }) => ({
        ...state,
        alerts: state.alerts.map(a => a.id === id ? { ...a, status: AlertStatus.Resolved, resolvedAt: new Date() } : a),
        summary: state.summary ? {
            ...state.summary,
            activeAlerts: Math.max(0, state.summary.activeAlerts - 1)
        } : null
    })),
    on(AlertsActions.acknowledgeAlertSuccess, (state, { id }) => ({
        ...state,
        alerts: state.alerts.map(a => a.id === id ? { ...a, status: AlertStatus.Acknowledged } : a)
    })),
    on(AlertsActions.archiveResolvedAlertsSuccess, (state) => ({
        ...state,
        alerts: state.alerts.filter(a => a.status !== AlertStatus.Resolved)
    })),
    on(AlertsActions.loadAlertSummarySuccess, (state, { summary }) => ({
        ...state,
        summary
    })),
    on(AlertsActions.markAlertsAsRead, (state) => ({
        ...state,
        unreadCount: 0
    }))
);
