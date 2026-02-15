import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { Store } from '@ngrx/store';
import * as AlertsActions from '../../../core/store/alerts/alerts.actions';
import * as AlertsSelectors from '../../../core/store/alerts/alerts.selectors';
import { AlertDto, AlertStatus, AlertSeverity } from '../../../core/models/alert.models';
import { AppTableComponent, AppTableColumn } from '../../../shared/components/app-table/app-table.component';

@Component({
  selector: 'app-alert-list',
  standalone: true,
  imports: [
    CommonModule,
    AppTableComponent,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatCardModule,
    MatSelectModule,
    MatFormFieldModule,
    MatTooltipModule,
    FormsModule
  ],
  templateUrl: './alert-list.component.html',
  styleUrl: './alert-list.component.css'
})
export class AlertListComponent implements OnInit {
  private store = inject(Store);

  readonly AlertStatus = AlertStatus;
  readonly AlertSeverity = AlertSeverity;

  // ─── Data State (Observables) ───
  alerts$ = this.store.select(AlertsSelectors.selectRecentAlerts);
  totalCount$ = this.store.select(AlertsSelectors.selectTotalCount);
  loading$ = this.store.select(AlertsSelectors.selectAlertsLoading);

  pageSize = 10;
  pageNumber = 1;

  statusFilter = '';
  severityFilter = '';

  sortBy = 'CreatedAt';
  sortDescending = true;

  columns: AppTableColumn[] = [
    { key: 'createdAt', label: 'Detected Time', type: 'date', sortable: true },
    { key: 'serverName', label: 'Originating node', type: 'template', sortable: true },
    { key: 'message', label: 'Event data', sortable: true },
    { key: 'severity', label: 'Priority level', type: 'template', sortable: true },
    { key: 'status', label: 'Current status', type: 'template', sortable: true },
    { key: 'actions', label: '', type: 'template', style: { 'text-align': 'right' } }
  ];

  ngOnInit(): void {
    this.loadAlerts();
  }

  loadAlerts(): void {
    this.store.dispatch(AlertsActions.loadAlerts({
      page: this.pageNumber,
      pageSize: this.pageSize,
      status: this.statusFilter || undefined,
      severity: this.severityFilter || undefined
      // Note: Sort mapping should be handled by backend or adapted if local
    }));
  }

  onPageChange(event: PageEvent): void {
    this.pageNumber = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadAlerts();
  }

  onSortChange(sort: Sort): void {
    this.sortBy = sort.active;
    this.sortDescending = sort.direction === 'desc';
    this.pageNumber = 1;
    this.loadAlerts();
  }

  applyFilter(): void {
    this.pageNumber = 1;
    this.loadAlerts();
  }

  acknowledge(id: number): void {
    this.store.dispatch(AlertsActions.acknowledgeAlert({ id }));
  }

  resolve(id: number): void {
    this.store.dispatch(AlertsActions.resolveAlert({ id, resolution: 'Resolved via dashboard' }));
  }

  archiveResolved(): void {
    this.store.dispatch(AlertsActions.archiveResolvedAlerts());
  }
}
