import { createAction, props } from '@ngrx/store';
import { ServerDto, PagedResult } from '../../../core/models/server.models';

export const loadServers = createAction(
    '[Servers] Load Servers',
    props<{ pageNumber?: number; pageSize?: number; searchTerm?: string; sortBy?: string; sortDescending?: boolean }>()
);

export const loadServersSuccess = createAction(
    '[Servers] Load Servers Success',
    props<{ result: PagedResult<ServerDto> }>()
);

export const loadServersFailure = createAction(
    '[Servers] Load Servers Failure',
    props<{ error: string }>()
);

export const selectServer = createAction(
    '[Servers] Select Server',
    props<{ id: number }>()
);

export const updateServerStatus = createAction(
    '[Servers] Update Server Status',
    props<{ id: number; status: string; cpuUsage?: number; memoryUsage?: number; diskUsage?: number }>()
);

export const loadServerDetails = createAction(
    '[Servers] Load Server Details',
    props<{ id: number }>()
);

export const loadServerDetailsSuccess = createAction(
    '[Servers] Load Server Details Success',
    props<{ server: ServerDto }>()
);

export const loadServerDetailsFailure = createAction(
    '[Servers] Load Server Details Failure',
    props<{ error: string }>()
);
