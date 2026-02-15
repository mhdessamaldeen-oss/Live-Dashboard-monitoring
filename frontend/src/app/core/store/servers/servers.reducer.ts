import { createReducer, on } from '@ngrx/store';
import * as ServersActions from './servers.actions';
import { ServerDto } from '../../../core/models/server.models';

export interface ServersState {
    servers: ServerDto[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
    loading: boolean;
    error: string | null;
    selectedServerId: number | null;
    currentServer: ServerDto | null;
}

export const initialServersState: ServersState = {
    servers: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
    totalPages: 0,
    loading: false,
    error: null,
    selectedServerId: null,
    currentServer: null
};

export const serversReducer = createReducer(
    initialServersState,
    on(ServersActions.loadServers, (state) => ({
        ...state,
        loading: true,
        error: null
    })),
    on(ServersActions.loadServersSuccess, (state, { result }) => ({
        ...state,
        servers: result.items,
        totalCount: result.totalCount,
        page: result.page,
        totalPages: result.totalPages,
        loading: false
    })),
    on(ServersActions.loadServersFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    })),
    on(ServersActions.selectServer, (state, { id }) => ({
        ...state,
        selectedServerId: id
    })),
    on(ServersActions.updateServerStatus, (state, { id, status, cpuUsage, memoryUsage, diskUsage }) => {
        const updatedServers = state.servers.map(s =>
            s.id === id ? { ...s, status, cpuUsage, memoryUsage, diskUsage, lastUpdate: new Date() } : s
        );
        let updatedCurrent = state.currentServer;
        if (state.currentServer && state.currentServer.id === id) {
            updatedCurrent = { ...state.currentServer, status, cpuUsage, memoryUsage, diskUsage, lastUpdate: new Date() };
        }
        return {
            ...state,
            servers: updatedServers,
            currentServer: updatedCurrent
        };
    }),
    on(ServersActions.loadServerDetails, (state) => ({
        ...state,
        loading: true,
        error: null
    })),
    on(ServersActions.loadServerDetailsSuccess, (state, { server }) => ({
        ...state,
        currentServer: server,
        loading: false
    })),
    on(ServersActions.loadServerDetailsFailure, (state, { error }) => ({
        ...state,
        loading: false,
        error
    }))
);
