import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AlertDto, AlertSummaryDto } from '../models/alert.models';
import { PagedResult } from '../models/server.models';
import { Result } from '../models/result.models';

@Injectable({
    providedIn: 'root'
})
export class AlertService {
    private readonly baseUrl = `${environment.apiUrl}/Alerts`;

    constructor(private http: HttpClient) { }

    getAlerts(params: { Page?: number; PageSize?: number; Status?: string; Severity?: string; ServerId?: number; SortBy?: string; SortDescending?: boolean }): Observable<PagedResult<AlertDto>> {
        let httpParams = new HttpParams();
        Object.keys(params).forEach(key => {
            const value = (params as any)[key];
            if (value !== undefined && value !== null) {
                httpParams = httpParams.set(key, value.toString());
            }
        });

        return this.http.get<Result<PagedResult<AlertDto>>>(this.baseUrl, { params: httpParams }).pipe(
            map(res => res.data)
        );
    }

    getAlertSummary(): Observable<AlertSummaryDto> {
        return this.http.get<Result<AlertSummaryDto>>(`${this.baseUrl}/summary`).pipe(
            map(res => res.data)
        );
    }

    acknowledgeAlert(id: number): Observable<void> {
        return this.http.post<Result<void>>(`${this.baseUrl}/${id}/acknowledge`, {}).pipe(
            map(() => undefined)
        );
    }

    resolveAlert(id: number, resolution: string): Observable<void> {
        return this.http.post<Result<void>>(`${this.baseUrl}/${id}/resolve`, { id, resolution }).pipe(
            map(() => undefined)
        );
    }

    archiveResolvedAlerts(): Observable<void> {
        return this.http.post<Result<void>>(`${this.baseUrl}/archive-resolved`, {}).pipe(
            map(() => undefined)
        );
    }
}
