import { Component, ContentChild, EventEmitter, Input, Output, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';

export interface AppTableColumn {
  key: string;
  label: string;
  type?: 'text' | 'date' | 'number' | 'status' | 'template';
  style?: any;
  sortable?: boolean;
}

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule
  ],
  templateUrl: './app-table.component.html',
  styleUrls: ['./app-table.component.css']
})
export class AppTableComponent {
  @Input() data: any[] = [];
  @Input() columns: AppTableColumn[] = [];
  @Input() loading = false;
  @Input() showPaginator = true;
  @Input() totalCount = 0;
  @Input() pageSize = 10;
  @Input() pageSizeOptions = [10, 25, 50, 100];

  @Output() page = new EventEmitter<PageEvent>();
  @Output() sort = new EventEmitter<Sort>();

  @ContentChild('cellTemplate') cellTemplate?: TemplateRef<any>;

  get columnKeys(): string[] {
    return this.columns.map(c => c.key);
  }
}
