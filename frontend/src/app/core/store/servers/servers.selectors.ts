import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ServersState } from './servers.reducer';

export const selectServersState = createFeatureSelector<ServersState>('servers');

export const selectAllServers = createSelector(
    selectServersState,
    (state) => state.servers
);

export const selectServersLoading = createSelector(
    selectServersState,
    (state) => state.loading
);

export const selectServersError = createSelector(
    selectServersState,
    (state) => state.error
);

export const selectSelectedServerId = createSelector(
    selectServersState,
    (state) => state.selectedServerId
);

export const selectCurrentServer = createSelector(
    selectServersState,
    (state) => state.currentServer
);

export const selectServerById = (id: number) => createSelector(
    selectAllServers,
    (servers) => servers.find(s => s.id === id)
);
