import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { environment } from '../../../../environments/environment';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { JobService, JobStatusDto } from '../../../core/services/job.service';
import { AppTableComponent, AppTableColumn } from '../../../shared/components/app-table/app-table.component';

import { Sort } from '@angular/material/sort';

@Component({
  selector: 'app-job-list',
  standalone: true,
  imports: [CommonModule, AppTableComponent, MatCardModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './job-list.component.html',
  styleUrl: './job-list.component.css'
})
export class JobListComponent implements OnInit {
  private jobService = inject(JobService);
  jobs: JobStatusDto[] = [];
  loading = false;
  hangfireUrl = environment.hangfireUrl;

  sortBy = 'name';
  sortDescending = false;

  columns: AppTableColumn[] = [
    { key: 'name', label: 'Infrastructure Task', type: 'template', sortable: true },
    { key: 'status', label: 'Engine Status', type: 'template', sortable: true },
    { key: 'cronExpression', label: 'Schedule Pattern', type: 'template', sortable: true },
    { key: 'lastRun', label: 'Last Execution', type: 'template', sortable: true },
    { key: 'nextRun', label: 'Next Eventual Run', type: 'template', sortable: true }
  ];

  ngOnInit(): void {
    this.loadJobs();
  }

  loadJobs(): void {
    this.loading = true;
    this.jobService.getJobs().subscribe({
      next: (data) => {
        this.jobs = this.sortData(data);
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  onSortChange(sort: Sort): void {
    this.sortBy = sort.active;
    this.sortDescending = sort.direction === 'desc';
    this.jobs = this.sortData(this.jobs);
  }

  private sortData(data: JobStatusDto[]): JobStatusDto[] {
    const sortedDetails = [...data].sort((a, b) => {
      const isAsc = !this.sortDescending;
      switch (this.sortBy) {
        case 'name': return this.compare(a.name, b.name, isAsc);
        case 'status': return this.compare(a.status, b.status, isAsc);
        case 'cronExpression': return this.compare(a.cronExpression, b.cronExpression, isAsc);
        case 'lastRun': return this.compare(a.lastRun || '', b.lastRun || '', isAsc);
        case 'nextRun': return this.compare(a.nextRun || '', b.nextRun || '', isAsc);
        default: return 0;
      }
    });
    return sortedDetails;
  }

  private compare(a: number | string, b: number | string, isAsc: boolean): number {
    return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
  }
}
