import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { NgApexchartsModule } from 'ng-apexcharts';
import * as AlertsActions from '../../../core/store/alerts/alerts.actions';
import * as AlertsSelectors from '../../../core/store/alerts/alerts.selectors';
import * as MetricsSelectors from '../../../core/store/metrics/metrics.selectors';
import * as ServersActions from '../../../core/store/servers/servers.actions';
import * as ServersSelectors from '../../../core/store/servers/servers.selectors';
import { Store } from '@ngrx/store';
import { selectTheme } from '../../../core/store/app.selectors';
import * as AppSelectors from '../../../core/store/app.selectors';

import { Subscription, filter, BehaviorSubject, of, switchMap } from 'rxjs';

import { ServerService } from '../../../core/services/server.service';
import { AlertService } from '../../../core/services/alert.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { SettingsService } from '../../../core/services/settings.service';
import { ServerDto } from '../../../core/models/server.models';
import { AlertDto, AlertSummaryDto, AlertStatus, AlertSeverity } from '../../../core/models/alert.models';
import { MetricDto, ServerDetailsDto } from '../../../core/models/metric.models';

import { StatCardComponent } from '../../../shared/components/stat-card/stat-card.component';

@Component({
    selector: 'app-dashboard-home',
    standalone: true,
    imports: [
        CommonModule,
        MatCardModule,
        MatIconModule,
        MatButtonModule,
        MatProgressBarModule,
        MatFormFieldModule,
        MatSelectModule,
        MatOptionModule,
        MatButtonToggleModule,
        MatProgressSpinnerModule,
        MatTooltipModule,
        MatChipsModule,
        NgApexchartsModule,
        StatCardComponent
    ],
    templateUrl: './dashboard-home.component.html',
    styleUrl: './dashboard-home.component.css'
})
export class DashboardHomeComponent implements OnInit, OnDestroy {
    private router = inject(Router);
    private serverService = inject(ServerService);
    private alertService = inject(AlertService);
    private signalRService = inject(SignalRService);
    private snackBar = inject(MatSnackBar);
    private settingsService = inject(SettingsService);
    private store = inject(Store);

    currentTheme: 'light' | 'dark' = 'light';

    // ─── Data State (Observables) ───
    metricsMode$ = this.settingsService.currentMode$;
    servers$ = this.store.select(ServersSelectors.selectAllServers);
    alerts$ = this.store.select(AlertsSelectors.selectRecentAlerts);
    alertSummary$ = this.store.select(AlertsSelectors.selectAlertSummary);
    serversLoading$ = this.store.select(ServersSelectors.selectServersLoading);
    alertsLoading$ = this.store.select(AlertsSelectors.selectAlertsLoading);
    unreadAlertsCount$ = this.store.select(AlertsSelectors.selectUnreadAlertsCount);

    private selectedServerIdSubject = new BehaviorSubject<number | null>(null);
    metrics$ = this.selectedServerIdSubject.pipe(
        switchMap(id => id ? this.store.select(MetricsSelectors.selectServerMetrics(id)) : of([]))
    );

    servers: ServerDto[] = [];
    filteredServers: ServerDto[] = [];
    alerts: AlertDto[] = [];
    alertSummary: AlertSummaryDto | null = null;

    // ─── UI State ───
    serverFilter = 'all';
    selectedServerId: number | null = null;
    metricsLoading = false;
    connectionStatus: 'connected' | 'disconnected' | 'connecting' = 'disconnected';

    // ─── Live Metrics Data ───
    selectedServerMetrics: MetricDto | null = null;
    selectedServerDetails: ServerDetailsDto | null = null;
    metricsHistory: MetricDto[] = [];

    // ─── ApexCharts Options ───
    cpuGaugeOptions: any = {};
    memGaugeOptions: any = {};
    diskGaugeOptions: any = {};
    networkChartOptions: any = {};

    private subscriptions: Subscription[] = [];

    // ─── Computed Properties ───
    get totalServers(): number {
        return this.servers.length;
    }

    get onlineServers(): number {
        return this.servers.filter(s => s.status?.toLowerCase() === 'online').length;
    }

    get criticalServers(): number {
        return this.servers.filter(s => s.status?.toLowerCase() === 'critical').length;
    }

    get selectedServerName(): string {
        if (!this.selectedServerId) return 'No Server Selected';
        const server = this.servers.find(s => s.id === this.selectedServerId);
        return server ? server.name.replace('[HOST] ', '') : 'Unknown Server';
    }

    ngOnInit(): void {
        this.initEmptyCharts();
        this.setupSignalR();

        // Load Initial Data via NgRx
        this.store.dispatch(ServersActions.loadServers({ pageNumber: 1, pageSize: 100 }));
        this.store.dispatch(AlertsActions.loadAlerts({ page: 1, pageSize: 50 }));
        this.store.dispatch(AlertsActions.loadAlertSummary());

        // Sync local copies for chart logic
        this.subscriptions.push(
            this.metricsMode$.subscribe(() => {
                // When mode changes, refresh servers from backend
                this.store.dispatch(ServersActions.loadServers({ pageNumber: 1, pageSize: 100 }));
            }),
            this.servers$.subscribe(s => {
                this.servers = s;
                this.applyServerFiltering();
                if (this.servers.length > 0 && (!this.selectedServerId || !this.servers.find(srv => srv.id === this.selectedServerId))) {
                    this.selectServer(this.servers[0].id, false);
                }
            }),
            this.alerts$.subscribe(a => this.alerts = a),
            this.alertSummary$.subscribe(as => this.alertSummary = as),
            this.store.select(selectTheme).subscribe(theme => {
                this.currentTheme = theme;
                this.updateChartsTheme();
            })
        );
    }

    ngOnDestroy(): void {
        this.subscriptions.forEach(sub => sub.unsubscribe());
        if (this.selectedServerId) {
            this.signalRService.leaveServerGroup(this.selectedServerId);
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  DATA LOADING
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private loadServerMetrics(serverId: number): void {
        this.metricsLoading = true;
        this.serverService.getServerDetails(serverId).subscribe({
            next: (details) => {
                this.selectedServerDetails = details;
                this.metricsHistory = details.latestMetrics || [];
                if (this.metricsHistory.length > 0) {
                    this.selectedServerMetrics = this.metricsHistory[0];
                }
                this.updateCharts();
                this.metricsLoading = false;
            },
            error: (err) => {
                console.error('Failed to load server metrics:', err);
                this.metricsLoading = false;
            }
        });
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  SIGNALR REAL-TIME
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private setupSignalR(): void {
        this.subscriptions.push(
            this.store.select(AppSelectors.selectConnectionStatus).subscribe(status => {
                this.connectionStatus = status;
            }),

            // Subscribe to Metrics from Store instead of Subject for consistency
            this.metrics$.subscribe(metrics => {
                if (!metrics || metrics.length === 0) {
                    this.selectedServerMetrics = null;
                    return;
                }

                // The store has oldest first ([...existing, new])
                // We want to slice the last few for charts
                const history = [...metrics].slice(-30);
                this.metricsHistory = history;
                this.selectedServerMetrics = history[history.length - 1]; // Latest is at the end

                this.updateCharts();
            })
        );
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  CHARTS (ApexCharts)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private initEmptyCharts(): void {
        const gaugeBase = {
            chart: { type: 'radialBar' as const, height: 200 },
            plotOptions: {
                radialBar: {
                    startAngle: -135,
                    endAngle: 135,
                    hollow: { size: '60%' },
                    track: { background: '#f0f4f8', strokeWidth: '100%' },
                    dataLabels: {
                        name: { show: true, offsetY: -10, fontSize: '12px', color: '#64748b' },
                        value: { show: true, fontSize: '24px', fontWeight: '700', offsetY: 5, formatter: (val: number) => `${Math.round(val)}%` }
                    }
                }
            }
        };

        this.cpuGaugeOptions = { ...gaugeBase, series: [0], labels: ['CPU'], colors: ['#6366f1'] };
        this.memGaugeOptions = { ...gaugeBase, series: [0], labels: ['Memory'], colors: ['#8b5cf6'] };
        this.diskGaugeOptions = { ...gaugeBase, series: [0], labels: ['Disk'], colors: ['#06b6d4'] };

        this.networkChartOptions = {
            chart: {
                type: 'area' as const,
                height: 200,
                toolbar: { show: false },
                animations: { enabled: true, easing: 'linear' as const, dynamicAnimation: { speed: 800 } }
            },
            series: [
                { name: 'Network In', data: [] },
                { name: 'Network Out', data: [] }
            ],
            xaxis: { type: 'datetime' as const, labels: { show: false } },
            yaxis: { labels: { formatter: (val: number) => `${val?.toFixed(1)} MB/s` }, min: 0 },
            stroke: { curve: 'smooth' as const, width: 2 },
            dataLabels: { enabled: false },
            fill: {
                type: 'gradient',
                gradient: { shadeIntensity: 1, opacityFrom: 0.5, opacityTo: 0.1, stops: [0, 90, 100] }
            },
            colors: ['#22c55e', '#f97316'],
            tooltip: { x: { format: 'HH:mm:ss' } },
            legend: { position: 'top' as const, fontSize: '12px' },
            theme: { mode: this.currentTheme }
        };
    }

    private updateChartsTheme(): void {
        const themeOption = { mode: this.currentTheme };
        this.cpuGaugeOptions = { ...this.cpuGaugeOptions, theme: themeOption };
        this.memGaugeOptions = { ...this.memGaugeOptions, theme: themeOption };
        this.diskGaugeOptions = { ...this.diskGaugeOptions, theme: themeOption };
        this.networkChartOptions = { ...this.networkChartOptions, theme: themeOption };
    }

    private updateCharts(): void {
        const cpu = this.selectedServerMetrics?.cpuUsage ?? 0;
        const mem = this.selectedServerMetrics?.memoryUsage ?? 0;
        const disk = this.selectedServerMetrics?.diskUsage ?? 0;

        this.cpuGaugeOptions = {
            ...this.cpuGaugeOptions,
            series: [cpu],
            colors: [cpu > 80 ? '#ef4444' : cpu > 60 ? '#f59e0b' : '#6366f1']
        };

        this.memGaugeOptions = {
            ...this.memGaugeOptions,
            series: [mem],
            colors: [mem > 80 ? '#ef4444' : mem > 60 ? '#f59e0b' : '#8b5cf6']
        };

        this.diskGaugeOptions = {
            ...this.diskGaugeOptions,
            series: [disk],
            colors: [disk > 80 ? '#ef4444' : disk > 60 ? '#f59e0b' : '#06b6d4']
        };

        if (this.metricsHistory.length > 0) {
            const networkInData = this.metricsHistory
                .map(m => ({ x: new Date(m.timestamp).getTime(), y: m.networkIn || 0 }));
            const networkOutData = this.metricsHistory
                .map(m => ({ x: new Date(m.timestamp).getTime(), y: m.networkOut || 0 }));

            this.networkChartOptions = {
                ...this.networkChartOptions,
                series: [
                    { name: 'Network In', data: networkInData },
                    { name: 'Network Out', data: networkOutData }
                ]
            };
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  UI ACTIONS
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    refresh(): void {
        this.store.dispatch(ServersActions.loadServers({ pageNumber: 1, pageSize: 100 }));
        this.store.dispatch(AlertsActions.loadAlerts({ page: 1, pageSize: 50 }));
        this.store.dispatch(AlertsActions.loadAlertSummary());
        this.snackBar.open('Dashboard refreshed', 'Close', { duration: 2000 });
    }

    private applyServerFiltering(): void {
        if (this.serverFilter === 'all') {
            this.filteredServers = [...this.servers];
        } else {
            this.filteredServers = this.servers.filter(s =>
                s.status?.toLowerCase() === this.serverFilter.toLowerCase()
            );
        }
    }

    filterServers(filterValue: string): void {
        this.serverFilter = filterValue;
        this.applyServerFiltering();
    }

    selectServer(serverId: number, showSnackbar = true): void {
        if (this.selectedServerId && this.selectedServerId !== serverId) {
            this.signalRService.leaveServerGroup(this.selectedServerId);
        }
        this.selectedServerId = serverId;
        this.selectedServerIdSubject.next(serverId);
        this.signalRService.joinServerGroup(serverId);
        this.loadServerMetrics(serverId);
        if (showSnackbar) {
            this.snackBar.open(`Monitoring ${this.getServerName(serverId)}`, 'Close', { duration: 2000 });
        }
    }

    onServerChange(serverId: number): void {
        if (serverId) {
            this.selectServer(serverId);
        }
    }

    refreshAlerts(): void {
        this.store.dispatch(AlertsActions.loadAlerts({ page: 1, pageSize: 50 }));
        this.store.dispatch(AlertsActions.loadAlertSummary());
        this.snackBar.open('Alerts refreshed', 'Close', { duration: 2000 });
    }

    resolveAlert(alertId: number): void {
        this.store.dispatch(AlertsActions.resolveAlert({ id: alertId, resolution: 'Resolved via Dashboard' }));
    }

    navigateToServer(serverId: number): void {
        this.router.navigate(['/servers', serverId]);
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    //  HELPERS
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    getServerName(serverId: number): string {
        const server = this.servers.find(s => s.id === serverId);
        return server ? server.name.replace('[HOST] ', '') : 'Unknown Server';
    }

    getStatusIcon(status: string): string {
        switch (status?.toLowerCase()) {
            case 'online': return 'check_circle';
            case 'warning': return 'warning';
            case 'critical': return 'error';
            case 'offline': return 'offline_bolt';
            default: return 'help';
        }
    }

    getStatusClass(status: string): string {
        return status?.toLowerCase() || 'unknown';
    }

    getAlertIcon(severity: AlertSeverity): string {
        switch (severity) {
            case AlertSeverity.Critical: return 'error';
            case AlertSeverity.Warning: return 'warning';
            case AlertSeverity.Info: return 'info';
            default: return 'notifications';
        }
    }

    getMetricColor(value: number): string {
        if (value > 80) return '#ef4444';
        if (value > 60) return '#f59e0b';
        return '#22c55e';
    }

    getLastUpdateDisplay(): string {
        if (!this.selectedServerMetrics?.timestamp) return 'N/A';
        return new Date(this.selectedServerMetrics.timestamp).toLocaleTimeString();
    }

    trackByServerId(index: number, server: ServerDto): number {
        return server.id;
    }

    trackByAlertId(index: number, alert: AlertDto): number {
        return alert.id;
    }
}
