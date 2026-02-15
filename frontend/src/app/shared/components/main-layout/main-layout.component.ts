import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { SettingsService } from '../../../core/services/settings.service';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { map, shareReplay } from 'rxjs/operators';

import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';

import { Store } from '@ngrx/store';
import { toggleTheme, setTheme } from '../../../core/store/app.actions';
import { selectTheme } from '../../../core/store/app.selectors';
import * as AlertsActions from '../../../core/store/alerts/alerts.actions';
import * as AlertsSelectors from '../../../core/store/alerts/alerts.selectors';
import { AlertDto } from '../../../core/models/alert.models';
import { Roles } from '../../../core/models/auth.models';

@Component({
    selector: 'app-main-layout',
    standalone: true,
    imports: [
        CommonModule,
        RouterOutlet,
        RouterLink,
        RouterLinkActive,
        MatSidenavModule,
        MatToolbarModule,
        MatListModule,
        MatIconModule,
        MatButtonModule,
        MatMenuModule,
        MatBadgeModule,
        MatTooltipModule,
        MatSnackBarModule
    ],
    templateUrl: './main-layout.component.html',
    styleUrl: './main-layout.component.css'
})
export class MainLayoutComponent implements OnInit, OnDestroy {
    public authService = inject(AuthService);
    private signalRService = inject(SignalRService);
    public settingsService = inject(SettingsService);
    public readonly Roles = Roles;
    private snackBar = inject(MatSnackBar);
    private store = inject(Store);
    private breakpointObserver = inject(BreakpointObserver);

    isHandset$ = this.breakpointObserver.observe(Breakpoints.Handset)
        .pipe(
            map(result => result.matches),
            shareReplay()
        );

    currentUser$ = this.authService.currentUser$;
    connectionStatus$ = this.signalRService.connectionStatus$;
    theme$ = this.store.select(selectTheme);
    recentAlerts$ = this.store.select(AlertsSelectors.selectRecentAlerts);
    unreadCount$ = this.store.select(AlertsSelectors.selectUnreadAlertsCount);
    metricsMode$ = this.settingsService.currentMode$;
    isSystemSupported$ = this.settingsService.isSystemSupported$;
    osPlatform$ = this.settingsService.osPlatform$;

    ngOnInit(): void {
        this.signalRService.startConnection();
        // Detect saved theme
        const savedTheme = localStorage.getItem('theme') as 'light' | 'dark' | null;
        if (savedTheme) {
            this.store.dispatch(setTheme({ theme: savedTheme }));
            document.body.parentElement?.setAttribute('data-theme', savedTheme);
        }

        // Initial alert load
        this.store.dispatch(AlertsActions.loadAlerts({ page: 1, pageSize: 5 }));
        this.store.dispatch(AlertsActions.loadAlertSummary());

        // Listen for completed reports
        this.signalRService.reportReady$.subscribe(report => {
            if (report) {
                this.snackBar.open(`Report "${report.reportTitle}" is ready for download`, 'View', {
                    duration: 6000,
                    panelClass: ['snackbar-success']
                }).onAction().subscribe(() => {
                    // Navigate to reports or open download
                });
            }
        });
    }

    ngOnDestroy(): void {
        this.signalRService.stopConnection();
    }

    toggleTheme(): void {
        this.store.dispatch(toggleTheme());
    }

    markAllAsRead(): void {
        this.store.dispatch(AlertsActions.markAlertsAsRead());
    }

    toggleMetricsProvider(isSystem: boolean): void {
        this.settingsService.setMode(isSystem ? 'System' : 'Mock').subscribe({
            error: (err) => {
                const msg = err?.error || 'System metrics not supported on this OS';
                this.snackBar.open(msg, 'Dismiss', {
                    duration: 5000,
                    panelClass: ['snackbar-warn']
                });
            }
        });
    }

    logout(): void {
        this.authService.logout();
        this.signalRService.stopConnection();
    }
}
