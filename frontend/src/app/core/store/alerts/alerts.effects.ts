import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, map, mergeMap } from 'rxjs/operators';
import { AlertService } from '../../../core/services/alert.service';
import * as AlertsActions from './alerts.actions';

@Injectable()
export class AlertsEffects {
    private actions$ = inject(Actions);
    private alertService = inject(AlertService);

    loadAlerts$ = createEffect(() =>
        this.actions$.pipe(
            ofType(AlertsActions.loadAlerts),
            mergeMap((action) =>
                this.alertService.getAlerts({
                    Page: action.page,
                    PageSize: action.pageSize,
                    Status: action.status,
                    Severity: action.severity,
                    ServerId: action.serverId
                }).pipe(
                    map((result) => AlertsActions.loadAlertsSuccess({ result })),
                    catchError((error) => of(AlertsActions.loadAlertsFailure({ error: error.message })))
                )
            )
        )
    );

    resolveAlert$ = createEffect(() =>
        this.actions$.pipe(
            ofType(AlertsActions.resolveAlert),
            mergeMap((action) =>
                this.alertService.resolveAlert(action.id, action.resolution).pipe(
                    map(() => AlertsActions.resolveAlertSuccess({ id: action.id })),
                    catchError((error) => of(AlertsActions.resolveAlertFailure({ error: error.message })))
                )
            )
        )
    );

    loadSummary$ = createEffect(() =>
        this.actions$.pipe(
            ofType(AlertsActions.loadAlertSummary),
            mergeMap(() =>
                this.alertService.getAlertSummary().pipe(
                    map((summary) => AlertsActions.loadAlertSummarySuccess({ summary })),
                    catchError((error) => of(AlertsActions.loadAlertSummaryFailure({ error: error.message })))
                )
            )
        )
    );

    acknowledgeAlert$ = createEffect(() =>
        this.actions$.pipe(
            ofType(AlertsActions.acknowledgeAlert),
            mergeMap((action) =>
                this.alertService.acknowledgeAlert(action.id).pipe(
                    map(() => AlertsActions.acknowledgeAlertSuccess({ id: action.id })),
                    catchError((error) => of(AlertsActions.acknowledgeAlertFailure({ error: error.message })))
                )
            )
        )
    );

    archiveResolvedAlerts$ = createEffect(() =>
        this.actions$.pipe(
            ofType(AlertsActions.archiveResolvedAlerts),
            mergeMap(() =>
                this.alertService.archiveResolvedAlerts().pipe(
                    map(() => AlertsActions.archiveResolvedAlertsSuccess()),
                    catchError((error) => of(AlertsActions.archiveResolvedAlertsFailure({ error: error.message })))
                )
            )
        )
    );

    // Refresh summary and list after alert state changes
    refreshAfterChange$ = createEffect(() =>
        this.actions$.pipe(
            ofType(
                AlertsActions.resolveAlertSuccess,
                AlertsActions.acknowledgeAlertSuccess,
                AlertsActions.archiveResolvedAlertsSuccess
            ),
            mergeMap(() => [
                AlertsActions.loadAlertSummary()
                // We might not want to reload the whole list here automatically if we handle it in the component
                // but usually it's cleaner to just reload or manually update.
            ])
        )
    );
}
