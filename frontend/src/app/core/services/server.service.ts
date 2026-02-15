import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ServerDto, PagedResult } from '../models/server.models';
import { ServerDetailsDto } from '../models/metric.models';
import { Result } from '../models/result.models';

@Injectable({
    providedIn: 'root'
})
export class ServerService {
    private readonly baseUrl = `${environment.apiUrl}/Servers`;

    constructor(private http: HttpClient) { }

    getServers(pageNumber = 1, pageSize = 10, searchTerm?: string, sortBy?: string, sortDescending: boolean = true): Observable<PagedResult<ServerDto>> {
        let params = new HttpParams()
            .set('Page', pageNumber.toString())
            .set('PageSize', pageSize.toString())
            .set('SortDescending', sortDescending.toString());

        if (sortBy) {
            params = params.set('SortBy', sortBy);
        }

        if (searchTerm) {
            params = params.set('Search', searchTerm);
        }

        return this.http.get<Result<PagedResult<ServerDto>>>(this.baseUrl, { params }).pipe(
            map(res => this.handleResult(res))
        );
    }

    getServerById(id: number): Observable<ServerDto> {
        return this.http.get<Result<ServerDto>>(`${this.baseUrl}/${id}`).pipe(
            map(res => this.handleResult(res))
        );
    }

    getServerDetails(id: number): Observable<ServerDetailsDto> {
        return this.http.get<Result<ServerDetailsDto>>(`${this.baseUrl}/${id}/details`).pipe(
            map(res => this.handleResult(res))
        );
    }

    getLatestMetrics(serverId: number): Observable<any> {
        return this.http.get<Result<any>>(`${this.baseUrl}/${serverId}/metrics/latest`).pipe(
            map(res => this.handleResult(res))
        );
    }

    createServer(server: Partial<ServerDto>): Observable<ServerDto> {
        return this.http.post<Result<ServerDto>>(this.baseUrl, server).pipe(
            map(res => this.handleResult(res))
        );
    }

    updateServer(id: number, server: Partial<ServerDto>): Observable<ServerDto> {
        return this.http.put<Result<ServerDto>>(`${this.baseUrl}/${id}`, server).pipe(
            map(res => this.handleResult(res))
        );
    }

    deleteServer(id: number): Observable<void> {
        return this.http.delete<Result<void>>(`${this.baseUrl}/${id}`).pipe(
            map(res => {
                this.handleResult(res);
                return undefined;
            })
        );
    }

    private handleResult<T>(result: Result<T>): T {
        if (result.isSuccess) {
            return result.data;
        }
        throw new Error(result.errors?.[0] || result.message || 'Operation failed');
    }
}
