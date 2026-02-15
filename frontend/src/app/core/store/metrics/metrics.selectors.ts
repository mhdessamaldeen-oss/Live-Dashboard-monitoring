import { createFeatureSelector, createSelector } from '@ngrx/store';
import { MetricsState } from './metrics.reducer';

export const selectMetricsState = createFeatureSelector<MetricsState>('metrics');

export const selectServerMetrics = (serverId: number) => createSelector(
    selectMetricsState,
    (state) => state.serverMetrics[serverId] || []
);

export const selectServerMetricsLoading = (serverId: number) => createSelector(
    selectMetricsState,
    (state) => state.loading[serverId] || false
);

export const selectServerMetricsError = (serverId: number) => createSelector(
    selectMetricsState,
    (state) => state.errors[serverId] || null
);
