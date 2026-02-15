import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, map, mergeMap } from 'rxjs/operators';
import { ServerService } from '../../../core/services/server.service';
import * as ServersActions from './servers.actions';

@Injectable()
export class ServersEffects {
    private actions$ = inject(Actions);
    private serverService = inject(ServerService);

    loadServers$ = createEffect(() =>
        this.actions$.pipe(
            ofType(ServersActions.loadServers),
            mergeMap((action) =>
                this.serverService.getServers(action.pageNumber, action.pageSize, action.searchTerm, action.sortBy, action.sortDescending).pipe(
                    map((result) => ServersActions.loadServersSuccess({ result })),
                    catchError((error) => of(ServersActions.loadServersFailure({ error: error.message })))
                )
            )
        )
    );

    loadServerDetails$ = createEffect(() =>
        this.actions$.pipe(
            ofType(ServersActions.loadServerDetails),
            mergeMap((action) =>
                this.serverService.getServerById(action.id).pipe(
                    map((server) => ServersActions.loadServerDetailsSuccess({ server })),
                    catchError((error) => of(ServersActions.loadServerDetailsFailure({ error: error.message })))
                )
            )
        )
    );
}
