import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface JobStatusDto {
    id: string;
    name: string;
    status: string;
    lastRun?: string;
    nextRun?: string;
    cronExpression: string;
}

@Injectable({
    providedIn: 'root'
})
export class JobService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/jobs`;

    getJobs(): Observable<JobStatusDto[]> {
        return this.http.get<JobStatusDto[]>(this.apiUrl);
    }
}
