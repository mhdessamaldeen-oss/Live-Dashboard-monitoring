import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { ScheduleDialogComponent } from '../schedule-dialog/schedule-dialog.component';

import { ReportService } from '../../../core/services/report.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { ReportDto, CreateReportRequest, ReportTemplateDto, ScheduledReportDto } from '../../../core/models/report.models';
import { PagedResult, ServerDto } from '../../../core/models/server.models';
import { ServerService } from '../../../core/services/server.service';
import { PageEvent } from '@angular/material/paginator';
import { AppTableComponent, AppTableColumn } from '../../../shared/components/app-table/app-table.component';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Actions, ofType } from '@ngrx/effects';
import * as AppActions from '../../../core/store/app.actions';

@Component({
  selector: 'app-report-list',
  standalone: true,
  imports: [
    CommonModule,
    AppTableComponent,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatCardModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatOptionModule,
    MatProgressBarModule,
    FormsModule
  ],
  templateUrl: './report-list.component.html',
  styleUrl: './report-list.component.css'
})
export class ReportListComponent implements OnInit, OnDestroy {
  private reportService = inject(ReportService);
  private signalRService = inject(SignalRService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private serverService = inject(ServerService);
  private actions$ = inject(Actions);

  private subscriptions: Subscription = new Subscription();

  serversList: ServerDto[] = [];
  selectedServerId: number | null = null;

  reports: ReportDto[] = [];
  filteredReports: ReportDto[] = [];
  totalCount = 0;
  pageSize = 10;
  pageNumber = 1;
  loading = false;

  // Filters
  statusFilter = 'all';
  typeFilter = 'all';

  // Templates
  reportTemplates: ReportTemplateDto[] = [];
  filteredTemplates: ReportTemplateDto[] = [];

  // Scheduled reports
  scheduledReports: ScheduledReportDto[] = [];

  // Statistics
  reportStats = {
    totalReports: 0,
    scheduledReports: 0,
    downloadsToday: 0,
    successRate: 0
  };

  columns: AppTableColumn[] = [
    { key: 'reportName', label: 'Report Title', type: 'template' },
    { key: 'serverName', label: 'Server', type: 'text' },
    { key: 'status', label: 'Status', type: 'template' },
    { key: 'size', label: 'Size', type: 'template' },
    { key: 'createdAt', label: 'Created', type: 'date' },
    { key: 'actions', label: '', type: 'template', style: { 'text-align': 'right' } }
  ];

  ngOnInit(): void {
    // Start SignalR connection
    this.signalRService.startConnection();

    // Refresh reports when notification arrives via NgRx
    this.subscriptions.add(
      this.actions$.pipe(ofType(AppActions.reportGenerated)).subscribe(() => {
        this.loadReports();
        this.loadStats();
      })
    );

    this.loadReports();
    this.initializeTemplates();
    this.loadSchedules();
    this.loadServers();
    this.loadStats();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
    // specific to this component's needs, or leave connection open globaly?
    // For now, let's keep it global or standard
  }

  loadStats(): void {
    this.reportService.getStats().subscribe({
      next: (stats) => {
        this.reportStats = stats;
      }
    });
  }

  loadSchedules(): void {
    this.reportService.getSchedules().subscribe({
      next: (schedules) => {
        this.scheduledReports = schedules;
      }
    });
  }

  loadServers(): void {
    this.serverService.getServers(1, 100).subscribe({
      next: (result) => {
        this.serversList = result.items;
        if (this.serversList.length > 0) {
          this.selectedServerId = this.serversList[0].id;
        }
      }
    });
  }

  loadReports(): void {
    this.loading = true;
    this.reportService.getReports(this.pageNumber, this.pageSize).subscribe({
      next: (result: PagedResult<ReportDto>) => {
        this.reports = result.items;
        this.filteredReports = result.items;
        this.totalCount = result.totalCount;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Failed to load reports', 'Close', { duration: 3000 });
      }
    });
  }

  private initializeTemplates(): void {
    this.reportTemplates = [
      {
        id: 1,
        name: 'System Performance Report',
        description: 'Comprehensive analysis of system performance metrics and trends',
        category: 'performance',
        features: ['CPU Analysis', 'Memory Trends', 'Disk Usage', 'Network Stats'],
        parameters: {
          timeRange: '24h',
          includeCharts: true,
          includeRecommendations: true
        }
      },
      {
        id: 2,
        name: 'Security Audit Report',
        description: 'Security assessment and vulnerability analysis',
        category: 'security',
        features: ['Security Scan', 'Access Logs', 'Threat Analysis', 'Compliance Check'],
        parameters: {
          scanType: 'full',
          includeRecommendations: true
        }
      },
      {
        id: 3,
        name: 'Infrastructure Summary',
        description: 'High-level overview of infrastructure health and status',
        category: 'summary',
        features: ['Server Status', 'Alert Summary', 'Capacity Planning', 'SLA Metrics'],
        parameters: {
          includeAlerts: true,
          includeMetrics: true
        }
      },
      {
        id: 4,
        name: 'Compliance Report',
        description: 'Regulatory compliance and audit trail documentation',
        category: 'compliance',
        features: ['Audit Trail', 'Policy Compliance', 'Risk Assessment', 'Documentation'],
        parameters: {
          complianceType: 'SOX',
          includeAuditTrail: true
        }
      },
      {
        id: 5,
        name: 'Custom Analytics',
        description: 'Custom report builder with flexible parameters',
        category: 'custom',
        features: ['Custom Metrics', 'Flexible Filters', 'Export Options', 'Scheduling'],
        parameters: {
          customMetrics: [],
          filters: {},
          exportFormat: 'PDF'
        }
      },
      {
        id: 6,
        name: 'Cost Analysis Report',
        description: 'Detailed cost breakdown and optimization recommendations',
        category: 'financial',
        features: ['Cost Breakdown', 'Resource Usage', 'Optimization', 'Forecasting'],
        parameters: {
          timeRange: 'monthly',
          includeForecasting: true
        }
      }
    ];
    this.filteredTemplates = [...this.reportTemplates];
  }

  onPageChange(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadReports();
  }

  filterReports(): void {
    this.filteredReports = this.reports.filter(report => {
      const statusMatch = this.statusFilter === 'all' || report.status.toLowerCase() === this.statusFilter.toLowerCase();
      return statusMatch;
    });
  }

  filterTemplates(searchTerm: string): void {
    if (!searchTerm) {
      this.filteredTemplates = [...this.reportTemplates];
    } else {
      const term = searchTerm.toLowerCase();
      this.filteredTemplates = this.reportTemplates.filter(template =>
        template.name.toLowerCase().includes(term) ||
        template.description.toLowerCase().includes(term) ||
        template.category.toLowerCase().includes(term)
      );
    }
  }

  // Template actions
  getTemplateIcon(category: string): string {
    switch (category) {
      case 'performance': return 'speed';
      case 'security': return 'security';
      case 'summary': return 'summarize';
      case 'compliance': return 'gavel';
      case 'custom': return 'settings';
      case 'financial': return 'attach_money';
      default: return 'description';
    }
  }

  previewTemplate(template: ReportTemplateDto): void {
    this.snackBar.open(`Previewing ${template.name}...`, 'Close', { duration: 2000 });
    // Implementation for template preview
  }

  useTemplate(template: ReportTemplateDto): void {
    const request: CreateReportRequest = {
      title: template.name,
      serverId: this.selectedServerId || this.serversList[0]?.id || 1,
      description: template.description,
      dateRangeStart: new Date(Date.now() - 86400000).toISOString(), // Last 24h
      dateRangeEnd: new Date().toISOString()
    };

    this.reportService.requestReport(request).subscribe({
      next: () => {
        this.snackBar.open('Report generation started...', 'Close', { duration: 3000 });
        this.loadReports();
        this.loadStats();
      },
      error: () => {
        this.snackBar.open('Failed to request report', 'Close', { duration: 3000 });
      }
    });
  }

  // Quick reports
  generateQuickReport(type: string): void {
    const now = new Date();
    const start = new Date();
    start.setHours(start.getHours() - 24);

    const request: CreateReportRequest = {
      serverId: this.selectedServerId || this.serversList[0]?.id || 1,
      title: `Quick ${type.charAt(0).toUpperCase() + type.slice(1)} Report`,
      description: `Auto-generated ${type} report for the last 24 hours.`,
      dateRangeStart: start.toISOString(),
      dateRangeEnd: now.toISOString()
    };

    this.reportService.requestReport(request).subscribe({
      next: () => {
        this.snackBar.open(`${type.charAt(0).toUpperCase() + type.slice(1)} report scheduled`, 'Close', { duration: 3000 });
        this.refreshReports();
        this.loadStats();
      },
      error: () => {
        this.snackBar.open('Failed to request quick report', 'Close', { duration: 3000 });
      }
    });
  }

  // Report actions
  getReportIcon(type: string): string {
    switch (type.toLowerCase()) {
      case 'summary': return 'summarize';
      case 'performance': return 'speed';
      case 'security': return 'security';
      case 'compliance': return 'gavel';
      case 'custom': return 'settings';
      default: return 'description';
    }
  }

  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'completed': return 'check_circle';
      case 'processing': return 'autorenew';
      case 'failed': return 'error';
      case 'scheduled': return 'schedule';
      default: return 'help_outline';
    }
  }

  downloadReport(id: number, title: string): void {
    this.reportService.downloadReport(id).subscribe((blob: Blob) => {
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${title.replace(/\s+/g, '_')}_${id}.csv`;
      link.click();
      window.URL.revokeObjectURL(url);

      this.reportStats.downloadsToday++;
      this.snackBar.open('Report downloaded successfully', 'Close', { duration: 3000 });
    });
  }

  shareReport(id: number): void {
    this.snackBar.open('Opening share dialog...', 'Close', { duration: 2000 });
    // Implementation for sharing functionality
  }

  deleteReport(id: number): void {
    if (confirm('Are you sure you want to delete this report?')) {
      this.reportService.deleteReport(id).subscribe({
        next: () => {
          this.snackBar.open('Report deleted successfully', 'Close', { duration: 3000 });
          this.loadReports();
        },
        error: () => {
          this.snackBar.open('Failed to delete report', 'Close', { duration: 3000 });
        }
      });
    }
  }

  viewReportDetails(id: number): void {
    this.router.navigate(['/reports', id]);
  }

  // Scheduled reports actions
  openScheduleDialog(): void {
    const dialogRef = this.dialog.open(ScheduleDialogComponent, {
      width: '500px',
      data: this.serversList,
      panelClass: 'glass-dialog'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.reportService.createSchedule(result).subscribe({
          next: () => {
            this.snackBar.open('Automated schedule created successfully', 'Close', { duration: 3000 });
            this.loadSchedules();
            this.loadStats();
          },
          error: (err) => {
            this.snackBar.open(err.message || 'Failed to create schedule', 'Close', { duration: 3000 });
          }
        });
      }
    });
  }

  editSchedule(id: number): void {
    this.snackBar.open('Editing schedule coming soon...', 'Close', { duration: 2000 });
  }

  pauseSchedule(id: number): void {
    this.reportService.toggleSchedule(id).subscribe({
      next: (res) => {
        this.snackBar.open(res.isActive ? 'Schedule resumed' : 'Schedule paused', 'Close', { duration: 3000 });
        this.loadSchedules();
      }
    });
  }

  deleteSchedule(id: number): void {
    if (confirm('Are you sure you want to delete this automated schedule?')) {
      this.reportService.deleteSchedule(id).subscribe({
        next: () => {
          this.snackBar.open('Schedule deleted', 'Close', { duration: 3000 });
          this.loadSchedules();
        }
      });
    }
  }

  // UI actions
  openReportWizard(): void {
    this.snackBar.open('Opening report wizard...', 'Close', { duration: 2000 });
    // Implementation for report wizard
  }

  refreshReports(): void {
    this.loadReports();
    this.snackBar.open('Reports refreshed', 'Close', { duration: 2000 });
  }

  // Legacy method for backward compatibility
  requestNewReport(): void {
    this.openReportWizard();
  }

  formatBytes(bytes: number | undefined): string {
    if (bytes === undefined || bytes === null || bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  getFrequencyLabel(cron: string): string {
    if (cron === '0 9 * * *') return 'Daily at 9 AM';
    if (cron === '0 0 * * 0') return 'Weekly on Sundays';
    if (cron === '0 0 1 * *') return 'Monthly';
    if (cron.startsWith('*/5')) return 'Every 5 Minutes';
    return 'Scheduled';
  }
}
