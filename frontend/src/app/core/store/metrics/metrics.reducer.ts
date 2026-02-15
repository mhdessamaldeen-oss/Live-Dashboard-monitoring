import { createReducer, on } from '@ngrx/store';
import * as MetricsActions from './metrics.actions';
import { MetricDto } from '../../../core/models/metric.models';

export interface MetricsState {
    serverMetrics: { [serverId: number]: MetricDto[] };
    loading: { [serverId: number]: boolean };
    errors: { [serverId: number]: string | null };
}

export const initialMetricsState: MetricsState = {
    serverMetrics: {},
    loading: {},
    errors: {}
};

const MAX_CHART_POINTS = 50;

export const metricsReducer = createReducer(
    initialMetricsState,
    on(MetricsActions.loadHistoricalMetrics, (state, { serverId }) => ({
        ...state,
        loading: { ...state.loading, [serverId]: true },
        errors: { ...state.errors, [serverId]: null }
    })),
    on(MetricsActions.loadHistoricalMetricsSuccess, (state, { serverId, metrics }) => ({
        ...state,
        serverMetrics: { ...state.serverMetrics, [serverId]: metrics },
        loading: { ...state.loading, [serverId]: false }
    })),
    on(MetricsActions.loadHistoricalMetricsFailure, (state, { serverId, error }) => ({
        ...state,
        loading: { ...state.loading, [serverId]: false },
        errors: { ...state.errors, [serverId]: error }
    })),
    on(MetricsActions.addRealTimeMetric, (state, { serverId, metric }) => {
        const existing = state.serverMetrics[serverId] || [];
        // Add new metric and keep only last MAX_CHART_POINTS
        const updated = [...existing, metric].slice(-MAX_CHART_POINTS);
        return {
            ...state,
            serverMetrics: { ...state.serverMetrics, [serverId]: updated }
        };
    }),
    on(MetricsActions.clearMetrics, (state, { serverId }) => {
        const { [serverId]: removed, ...rest } = state.serverMetrics;
        return {
            ...state,
            serverMetrics: rest
        };
    })
);
