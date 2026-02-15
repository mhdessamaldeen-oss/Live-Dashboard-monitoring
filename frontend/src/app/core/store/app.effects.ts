import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { ServerService } from '../services/server.service';
import { AlertService } from '../services/alert.service';
import * as AppActions from './app.actions';
import { catchError, map, mergeMap, tap } from 'rxjs/operators';
import { forkJoin, of } from 'rxjs';

import { SignalRService } from '../services/signalr.service';
import { Store } from '@ngrx/store';

import { selectTheme } from './app.selectors';
import { withLatestFrom } from 'rxjs/operators';

@Injectable()
export class AppEffects {
    private actions$ = inject(Actions);
    private serverService = inject(ServerService);
    private alertService = inject(AlertService);
    private signalRService = inject(SignalRService);
    private store = inject(Store);

    syncRealtime$ = createEffect(() =>
        this.signalRService.alerts$.pipe(
            tap(alert => alert && this.store.dispatch(AppActions.loadDashboard())),
            map(() => ({ type: 'NO_ACTION' }))
        ), { dispatch: false }
    );

    loadDashboard$ = createEffect(() =>
        this.actions$.pipe(
            ofType(AppActions.loadDashboard),
            mergeMap(() =>
                forkJoin({
                    servers: this.serverService.getServers(1, 10),
                    alerts: this.alertService.getAlerts({ Page: 1, PageSize: 5 }),
                    summary: this.alertService.getAlertSummary()
                }).pipe(
                    map(data => AppActions.loadDashboardSuccess({
                        serversCount: data.servers.totalCount,
                        alerts: data.alerts.items,
                        summary: data.summary
                    })),
                    catchError(error => of(AppActions.loadDashboardFailure({ error: error.message })))
                )
            )
        )
    );

    syncTheme$ = createEffect(() =>
        this.actions$.pipe(
            ofType(AppActions.toggleTheme, AppActions.setTheme),
            withLatestFrom(this.store.select(selectTheme)),
            tap(([action, theme]) => {
                document.body.parentElement?.setAttribute('data-theme', theme);
                localStorage.setItem('theme', theme);
            })
        ), { dispatch: false }
    );
}
