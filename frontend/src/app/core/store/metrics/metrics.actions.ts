import { createAction, props } from '@ngrx/store';
import { MetricDto } from '../../../core/models/metric.models';

export const loadHistoricalMetrics = createAction(
    '[Metrics] Load Historical Metrics',
    props<{ serverId: number; startTime?: string; endTime?: string; pageNumber?: number; pageSize?: number }>()
);

export const loadHistoricalMetricsSuccess = createAction(
    '[Metrics] Load Historical Metrics Success',
    props<{ serverId: number; metrics: MetricDto[] }>()
);

export const loadHistoricalMetricsFailure = createAction(
    '[Metrics] Load Historical Metrics Failure',
    props<{ serverId: number; error: string }>()
);

export const addRealTimeMetric = createAction(
    '[Metrics] Add Real Time Metric',
    props<{ serverId: number; metric: MetricDto }>()
);

export const clearMetrics = createAction(
    '[Metrics] Clear Metrics',
    props<{ serverId: number }>()
);
