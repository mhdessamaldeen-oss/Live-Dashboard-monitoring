import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { AlertDto } from '../../../core/models/alert.models';

@Component({
    selector: 'app-alerts-table',
    standalone: true,
    imports: [CommonModule, MatTableModule, MatIconModule],
    templateUrl: './alerts-table.component.html',
    styleUrls: ['./alerts-table.component.css']
})
export class AlertsTableComponent {
    @Input() alerts: AlertDto[] = [];
    displayedColumns = ['server', 'message', 'severity', 'time'];
}
