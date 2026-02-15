import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export interface MetricsProviderResponse {
    mode: string;
    isSystemSupported: boolean;
    osPlatform: string;
}

@Injectable({
    providedIn: 'root'
})
export class SettingsService {
    private readonly baseUrl = `${environment.apiUrl}/settings`;
    private modeSubject = new BehaviorSubject<string>('Mock');
    private systemSupportedSubject = new BehaviorSubject<boolean>(false);
    private osPlatformSubject = new BehaviorSubject<string>('');

    currentMode$ = this.modeSubject.asObservable();
    isSystemSupported$ = this.systemSupportedSubject.asObservable();
    osPlatform$ = this.osPlatformSubject.asObservable();

    getCurrentModeValue(): string {
        return this.modeSubject.value;
    }

    constructor(private http: HttpClient) {
        this.loadCurrentMode();
    }

    loadCurrentMode(): void {
        this.http.get<MetricsProviderResponse>(`${this.baseUrl}/metrics-provider`).subscribe({
            next: (res) => this.applyResponse(res),
            error: () => { }
        });
    }

    setMode(mode: string): Observable<MetricsProviderResponse> {
        return this.http.post<MetricsProviderResponse>(`${this.baseUrl}/metrics-provider`, { mode }).pipe(
            tap(res => this.applyResponse(res)),
            catchError((err) => {
                this.loadCurrentMode();
                return throwError(() => err);
            })
        );
    }

    private applyResponse(res: MetricsProviderResponse): void {
        this.modeSubject.next(res.mode);
        this.systemSupportedSubject.next(res.isSystemSupported);
        this.osPlatformSubject.next(res.osPlatform);
    }
}
