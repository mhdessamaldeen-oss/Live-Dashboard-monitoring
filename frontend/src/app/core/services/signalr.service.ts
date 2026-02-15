import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { Store } from '@ngrx/store';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from './auth.service';
import * as ServersActions from '../store/servers/servers.actions';
import * as MetricsActions from '../store/metrics/metrics.actions';
import * as AlertsActions from '../store/alerts/alerts.actions';

@Injectable({
    providedIn: 'root'
})
export class SignalRService {
    private hubConnection: signalR.HubConnection | null = null;

    private metricsSubject = new BehaviorSubject<any>(null);
    public metrics$ = this.metricsSubject.asObservable();

    private alertsSubject = new BehaviorSubject<any>(null);
    public alerts$ = this.alertsSubject.asObservable();

    private reportReadySubject = new BehaviorSubject<any>(null);
    public reportReady$ = this.reportReadySubject.asObservable();

    private connectionStatusSubject = new BehaviorSubject<'connected' | 'disconnected' | 'connecting'>('disconnected');
    public connectionStatus$ = this.connectionStatusSubject.asObservable();

    constructor(private authService: AuthService, private store: Store) { }

    public startConnection(): void {
        const token = this.authService.token;
        if (!token) return;

        this.connectionStatusSubject.next('connecting');

        this.hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${environment.hubUrl}/monitoring`, {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        this.hubConnection.onreconnecting(() => this.connectionStatusSubject.next('connecting'));
        this.hubConnection.onreconnected(() => this.connectionStatusSubject.next('connected'));
        this.hubConnection.onclose(() => this.connectionStatusSubject.next('disconnected'));

        this.hubConnection
            .start()
            .then(() => {
                console.log('Connection started');
                this.connectionStatusSubject.next('connected');
                this.joinDashboardGroup();
            })
            .catch(err => {
                console.log('Error while starting connection: ' + err);
                this.connectionStatusSubject.next('disconnected');
            });

        this.registerOnEvents();
    }

    public async joinDashboardGroup(): Promise<void> {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('JoinDashboardGroup');
        }
    }

    public async leaveDashboardGroup(): Promise<void> {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('LeaveDashboardGroup');
        }
    }

    public async joinServerGroup(serverId: number): Promise<void> {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('JoinServerGroup', serverId);
        }
    }

    public async leaveServerGroup(serverId: number): Promise<void> {
        if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
            await this.hubConnection.invoke('LeaveServerGroup', serverId);
        }
    }

    public stopConnection(): void {
        if (this.hubConnection) {
            this.hubConnection.stop();
            this.connectionStatusSubject.next('disconnected');
        }
    }

    private registerOnEvents(): void {
        if (!this.hubConnection) return;

        this.hubConnection.on('ReceiveMetricUpdate', (update) => {
            const metrics = update.metric;
            if (!metrics || !metrics.serverId) return;

            // Dispatch to stores
            this.store.dispatch(ServersActions.updateServerStatus({
                id: metrics.serverId,
                status: metrics.status || 'unknown',
                cpuUsage: metrics.cpuUsage ?? metrics.cpuUsagePercent,
                memoryUsage: metrics.memoryUsage ?? metrics.memoryUsagePercent,
                diskUsage: metrics.diskUsage
            }));

            this.store.dispatch(MetricsActions.addRealTimeMetric({
                serverId: metrics.serverId,
                metric: {
                    id: metrics.id || 0,
                    serverId: metrics.serverId,
                    cpuUsage: metrics.cpuUsage ?? metrics.cpuUsagePercent ?? 0,
                    memoryUsage: metrics.memoryUsage ?? metrics.memoryUsagePercent ?? 0,
                    diskUsage: metrics.diskUsage ?? 0,
                    networkIn: metrics.networkIn ?? metrics.networkInBytesPerSec ?? 0,
                    networkOut: metrics.networkOut ?? metrics.networkOutBytesPerSec ?? 0,
                    timestamp: metrics.timestamp || new Date().toISOString()
                }
            }));

            this.metricsSubject.next(update);
        });

        this.hubConnection.on('ReceiveAlertTriggered', (alert) => {
            this.store.dispatch(AlertsActions.addAlert({ alert }));
            this.alertsSubject.next(alert);
        });

        this.hubConnection.on('ReceiveAlertResolved', (alertId) => {
            // Depending on what 'alert' contains (DTO or just ID)
            // If it's the full alert DTO, we can update it in the store
            const id = typeof alertId === 'number' ? alertId : (alertId as any).id;
            this.store.dispatch(AlertsActions.resolveAlertSuccess({ id }));
        });

        this.hubConnection.on('ReceiveReportReady', (reportInfo) => {
            this.reportReadySubject.next(reportInfo);
        });
    }
}
