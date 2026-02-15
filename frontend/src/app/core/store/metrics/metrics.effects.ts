import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, map, mergeMap } from 'rxjs/operators';
import { MetricService } from '../../../core/services/metric.service';
import * as MetricsActions from './metrics.actions';

@Injectable()
export class MetricsEffects {
    private actions$ = inject(Actions);
    private metricService = inject(MetricService);

    loadHistoricalMetrics$ = createEffect(() =>
        this.actions$.pipe(
            ofType(MetricsActions.loadHistoricalMetrics),
            mergeMap((action) =>
                this.metricService.getMetrics(action.serverId, action.pageNumber, action.pageSize, action.startTime, action.endTime).pipe(
                    map((result) => MetricsActions.loadHistoricalMetricsSuccess({ serverId: action.serverId, metrics: result.items })),
                    catchError((error) => of(MetricsActions.loadHistoricalMetricsFailure({ serverId: action.serverId, error: error.message })))
                )
            )
        )
    );
}
