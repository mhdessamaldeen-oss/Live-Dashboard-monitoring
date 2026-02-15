import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NgApexchartsModule } from 'ng-apexcharts';
import { SignalRService } from '../../../core/services/signalr.service';
import { MetricDto } from '../../../core/models/metric.models';
import { Subscription, BehaviorSubject, of } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { AppTableComponent, AppTableColumn } from '../../../shared/components/app-table/app-table.component';
import { Store } from '@ngrx/store';
import { selectTheme } from '../../../core/store/app.selectors';
import * as ServersActions from '../../../core/store/servers/servers.actions';
import * as ServersSelectors from '../../../core/store/servers/servers.selectors';
import * as MetricsActions from '../../../core/store/metrics/metrics.actions';
import * as MetricsSelectors from '../../../core/store/metrics/metrics.selectors';

@Component({
    selector: 'app-server-details',
    standalone: true,
    imports: [
        CommonModule,
        RouterLink,
        MatCardModule,
        MatIconModule,
        MatButtonModule,
        MatChipsModule,
        MatDividerModule,
        MatProgressBarModule,
        MatProgressSpinnerModule,
        MatTooltipModule,
        NgApexchartsModule,
        AppTableComponent
    ],
    templateUrl: './server-details.component.html',
    styleUrl: './server-details.component.css'
})
export class ServerDetailsComponent implements OnInit, OnDestroy {
    private route = inject(ActivatedRoute);
    private signalRService = inject(SignalRService);
    private store = inject(Store);

    currentTheme: 'light' | 'dark' = 'light';
    private serverIdSubject = new BehaviorSubject<number | null>(null);

    // ─── Data State (Observables) ───
    server$ = this.store.select(ServersSelectors.selectCurrentServer);
    serversLoading$ = this.store.select(ServersSelectors.selectServersLoading);

    metrics$ = this.serverIdSubject.pipe(
        switchMap(id => id ? this.store.select(MetricsSelectors.selectServerMetrics(id)) : of([]))
    );

    // Simplified metrics subscription for local logic (charts)
    localMetrics: MetricDto[] = [];

    columns: AppTableColumn[] = [
        { key: 'timestamp', label: 'Time (UTC)', type: 'template' },
        { key: 'cpuUsage', label: 'CPU Util.', type: 'template' },
        { key: 'memoryUsage', label: 'Memory Util.', type: 'template' },
        { key: 'diskUsage', label: 'IO Performance', type: 'template' }
    ];

    public cpuChartOptions: any = { series: [], chart: {}, xaxis: {}, title: {} };
    public memChartOptions: any = { series: [], chart: {}, xaxis: {}, title: {} };

    private subscriptions: Subscription[] = [];

    ngOnInit(): void {
        const id = Number(this.route.snapshot.paramMap.get('id'));
        if (id) {
            this.serverIdSubject.next(id);

            // Initial Data Load
            this.store.dispatch(ServersActions.loadServerDetails({ id }));
            this.store.dispatch(MetricsActions.loadHistoricalMetrics({
                serverId: id,
                pageNumber: 1,
                pageSize: 30
            }));

            this.initCharts();
            this.signalRService.joinServerGroup(id);

            // Sync with Store
            this.subscriptions.push(
                this.metrics$.subscribe(m => {
                    this.localMetrics = m;
                    this.updateChartsData();
                }),
                this.store.select(selectTheme).subscribe(theme => {
                    this.currentTheme = theme;
                    this.updateChartsTheme();
                })
            );
        }
    }

    ngOnDestroy(): void {
        this.subscriptions.forEach(sub => sub.unsubscribe());
        const id = this.serverIdSubject.value;
        if (id) {
            this.signalRService.leaveServerGroup(id);
        }
    }

    initCharts(): void {
        const commonOptions = {
            chart: {
                height: 350,
                type: "area" as const,
                animations: { enabled: true, easing: 'linear' as const, dynamicAnimation: { speed: 1000 } },
                toolbar: { show: false }
            },
            dataLabels: { enabled: false },
            stroke: { curve: "smooth" as const, width: 2 },
            xaxis: { type: "datetime" as const },
            tooltip: { x: { format: "HH:mm:ss" } },
            fill: {
                type: 'gradient',
                gradient: { shadeIntensity: 1, opacityFrom: 0.7, opacityTo: 0.3, stops: [0, 90, 100] }
            }
        };

        this.cpuChartOptions = {
            ...commonOptions,
            series: [{ name: "CPU Usage %", data: [] }],
            title: { text: "CPU Usage History", align: 'left' as const },
            yaxis: { min: 0, max: 100, labels: { formatter: (val: number) => val.toFixed(1) + '%' } },
            colors: ['#1976d2']
        };

        this.memChartOptions = {
            ...commonOptions,
            series: [{ name: "Memory Usage %", data: [] }],
            title: { text: "Memory Usage History", align: 'left' as const },
            yaxis: { min: 0, max: 100, labels: { formatter: (val: number) => val.toFixed(1) + '%' } },
            colors: ['#9c27b0']
        };
    }

    updateChartsTheme(): void {
        const themeOption = { mode: this.currentTheme };
        this.cpuChartOptions = { ...this.cpuChartOptions, theme: themeOption };
        this.memChartOptions = { ...this.memChartOptions, theme: themeOption };
    }

    updateChartsData(): void {
        if (!this.localMetrics.length) return;

        const cpuData = this.localMetrics.map(m => ({ x: new Date(m.timestamp).getTime(), y: m.cpuUsage })).reverse();
        const memData = this.localMetrics.map(m => ({ x: new Date(m.timestamp).getTime(), y: m.memoryUsage })).reverse();

        this.cpuChartOptions = {
            ...this.cpuChartOptions,
            series: [{ name: "CPU Usage %", data: cpuData }]
        };

        this.memChartOptions = {
            ...this.memChartOptions,
            series: [{ name: "Memory Usage %", data: memData }]
        };
    }
}
