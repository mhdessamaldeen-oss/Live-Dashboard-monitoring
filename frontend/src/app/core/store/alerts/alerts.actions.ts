import { createAction, props } from '@ngrx/store';
import { AlertDto, AlertSummaryDto } from '../../../core/models/alert.models';
import { PagedResult } from '../../../core/models/server.models';

export const loadAlerts = createAction(
    '[Alerts] Load Alerts',
    props<{ page?: number; pageSize?: number; status?: string; severity?: string; serverId?: number }>()
);

export const loadAlertsSuccess = createAction(
    '[Alerts] Load Alerts Success',
    props<{ result: PagedResult<AlertDto> }>()
);

export const loadAlertsFailure = createAction(
    '[Alerts] Load Alerts Failure',
    props<{ error: string }>()
);

export const addAlert = createAction(
    '[Alerts] Add Alert',
    props<{ alert: AlertDto }>()
);

export const resolveAlert = createAction(
    '[Alerts] Resolve Alert',
    props<{ id: number; resolution: string }>()
);

export const resolveAlertSuccess = createAction(
    '[Alerts] Resolve Alert Success',
    props<{ id: number }>()
);

export const resolveAlertFailure = createAction(
    '[Alerts] Resolve Alert Failure',
    props<{ error: string }>()
);

export const loadAlertSummary = createAction('[Alerts] Load Alert Summary');

export const loadAlertSummarySuccess = createAction(
    '[Alerts] Load Alert Summary Success',
    props<{ summary: AlertSummaryDto }>()
);

export const loadAlertSummaryFailure = createAction(
    '[Alerts] Load Alert Summary Failure',
    props<{ error: string }>()
);

export const markAlertsAsRead = createAction('[Alerts] Mark Alerts As Read');

export const acknowledgeAlert = createAction(
    '[Alerts] Acknowledge Alert',
    props<{ id: number }>()
);

export const acknowledgeAlertSuccess = createAction(
    '[Alerts] Acknowledge Alert Success',
    props<{ id: number }>()
);

export const acknowledgeAlertFailure = createAction(
    '[Alerts] Acknowledge Alert Failure',
    props<{ error: string }>()
);

export const archiveResolvedAlerts = createAction('[Alerts] Archive Resolved Alerts');

export const archiveResolvedAlertsSuccess = createAction('[Alerts] Archive Resolved Alerts Success');

export const archiveResolvedAlertsFailure = createAction(
    '[Alerts] Archive Resolved Alerts Failure',
    props<{ error: string }>()
);
