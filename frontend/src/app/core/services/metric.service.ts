import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { MetricDto } from '../models/metric.models';
import { Result } from '../models/result.models';
import { PagedResult } from '../models/server.models';

@Injectable({
    providedIn: 'root'
})
export class MetricService {
    constructor(private http: HttpClient) { }

    getMetrics(serverId: number, pageNumber = 1, pageSize = 50, startTime?: string, endTime?: string): Observable<PagedResult<MetricDto>> {
        let params = new HttpParams()
            .set('ServerId', serverId.toString())
            .set('PageNumber', pageNumber.toString())
            .set('PageSize', pageSize.toString());

        if (startTime) params = params.set('StartTime', startTime);
        if (endTime) params = params.set('EndTime', endTime);

        return this.http.get<Result<PagedResult<MetricDto>>>(`${environment.apiUrl}/servers/${serverId}/metrics`, { params }).pipe(
            map(res => this.handleResult(res))
        );
    }

    getLatestMetrics(serverId: number): Observable<MetricDto> {
        return this.http.get<Result<MetricDto>>(`${environment.apiUrl}/servers/${serverId}/metrics/latest`).pipe(
            map(res => this.handleResult(res))
        );
    }

    private handleResult<T>(result: Result<T>): T {
        if (result.isSuccess) {
            return result.data;
        }
        throw new Error(result.errors?.[0] || result.message || 'Operation failed');
    }
}
