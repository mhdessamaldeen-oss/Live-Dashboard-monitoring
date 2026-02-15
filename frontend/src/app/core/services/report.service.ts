import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ReportDto, CreateReportRequest, ScheduledReportDto, CreateReportScheduleRequest } from '../models/report.models';
import { PagedResult } from '../models/server.models';
import { Result } from '../models/result.models';

@Injectable({
    providedIn: 'root'
})
export class ReportService {
    private readonly baseUrl = `${environment.apiUrl}/Reports`;

    constructor(private http: HttpClient) { }

    getReports(pageNumber = 1, pageSize = 10): Observable<PagedResult<ReportDto>> {
        return this.http.get<Result<PagedResult<ReportDto>>>(this.baseUrl, {
            params: { Page: pageNumber.toString(), PageSize: pageSize.toString() }
        }).pipe(
            map(res => res.data)
        );
    }

    getStats(): Observable<any> {
        return this.http.get<Result<any>>(`${this.baseUrl}/stats`).pipe(
            map(res => res.data)
        );
    }

    requestReport(request: CreateReportRequest): Observable<ReportDto> {
        return this.http.post<Result<ReportDto>>(this.baseUrl, request).pipe(
            map(res => res.data)
        );
    }

    downloadReport(id: number): Observable<Blob> {
        return this.http.get(`${this.baseUrl}/${id}/download`, { responseType: 'blob' });
    }

    deleteReport(id: number): Observable<void> {
        return this.http.delete<Result<void>>(`${this.baseUrl}/${id}`).pipe(
            map(() => undefined)
        );
    }

    // Schedule methods
    getSchedules(): Observable<ScheduledReportDto[]> {
        return this.http.get<ScheduledReportDto[]>(`${environment.apiUrl}/ReportSchedules`);
    }

    createSchedule(request: CreateReportScheduleRequest): Observable<ScheduledReportDto> {
        return this.http.post<ScheduledReportDto>(`${environment.apiUrl}/ReportSchedules`, request);
    }

    toggleSchedule(id: number): Observable<{ isActive: boolean }> {
        return this.http.post<{ isActive: boolean }>(`${environment.apiUrl}/ReportSchedules/${id}/toggle`, {});
    }

    deleteSchedule(id: number): Observable<void> {
        return this.http.delete<void>(`${environment.apiUrl}/ReportSchedules/${id}`);
    }
}
