import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import { RouterLink } from '@angular/router';
import { ServerService } from '../../../core/services/server.service';
import { ServerDto, PagedResult } from '../../../core/models/server.models';
import { FormsModule } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { AuthService } from '../../../core/services/auth.service';
import { SettingsService } from '../../../core/services/settings.service';

import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { ClipboardModule } from '@angular/cdk/clipboard';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { ProvisionDialogComponent } from '../provision-dialog/provision-dialog.component';
import { AppTableComponent, AppTableColumn } from '../../../shared/components/app-table/app-table.component';

@Component({
    selector: 'app-server-list',
    standalone: true,
    imports: [
        CommonModule,
        AppTableComponent,
        MatButtonModule,
        MatIconModule,
        MatChipsModule,
        MatInputModule,
        MatFormFieldModule,
        MatCardModule,
        MatTooltipModule,
        MatProgressSpinnerModule,
        MatMenuModule,
        RouterLink,
        FormsModule,
        MatSnackBarModule,
        ClipboardModule,
        MatDialogModule
    ],
    templateUrl: './server-list.component.html',
    styleUrl: './server-list.component.css'
})
export class ServerListComponent implements OnInit {
    private serverService = inject(ServerService);
    private snackBar = inject(MatSnackBar);
    private dialog = inject(MatDialog);
    public authService = inject(AuthService);
    private settingsService = inject(SettingsService);

    servers: ServerDto[] = [];
    totalCount = 0;
    pageSize = 10;
    pageNumber = 1;
    searchTerm = '';
    loading = false;
    sortBy = 'Id';
    sortDescending = false;

    columns: AppTableColumn[] = [
        { key: 'name', label: 'Node Details', type: 'template', sortable: true },
        { key: 'ipAddress', label: 'Interface', type: 'template', sortable: true },
        { key: 'operatingSystem', label: 'Platform', sortable: true },
        { key: 'location', label: 'Zone', sortable: true },
        { key: 'status', label: 'Health', type: 'template', sortable: true },
        { key: 'actions', label: '', type: 'template', style: { 'text-align': 'right' } }
    ];

    ngOnInit(): void {
        this.loadServers();
        // Refresh when metrics mode changed
        this.settingsService.currentMode$.subscribe(() => {
            this.loadServers();
        });
    }

    loadServers(): void {
        this.loading = true;
        this.serverService.getServers(this.pageNumber, this.pageSize, this.searchTerm, this.sortBy, this.sortDescending).subscribe({
            next: (result: PagedResult<ServerDto>) => {
                this.servers = result.items;
                this.totalCount = result.totalCount;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    copyIp(ip: string): void {
        this.snackBar.open(`IP Address ${ip} copied to clipboard`, 'OK', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top'
        });
    }

    onPageChange(event: PageEvent): void {
        this.pageNumber = event.pageIndex + 1;
        this.pageSize = event.pageSize;
        this.loadServers();
    }

    onSortChange(sort: Sort): void {
        this.sortBy = sort.active;
        this.sortDescending = sort.direction === 'desc';
        this.pageNumber = 1;
        this.loadServers();
    }

    applyFilter(): void {
        this.pageNumber = 1;
        this.loadServers();
    }

    provisionServer(): void {
        this.openServerDialog();
    }

    editServer(server: ServerDto): void {
        this.openServerDialog(server);
    }

    private openServerDialog(server?: ServerDto): void {
        const dialogRef = this.dialog.open(ProvisionDialogComponent, {
            width: '640px',
            data: server || null,
            disableClose: true
        });

        dialogRef.afterClosed().subscribe((result: ServerDto | null) => {
            if (result) {
                const message = server
                    ? `Node configuration updated for ${result.name}`
                    : `Infrastructure Node ${result.name} Provisioned Successfully`;

                this.snackBar.open(message, 'Dismiss', {
                    duration: 4000
                });
                this.loadServers();
            }
        });
    }

    deleteServer(id: number): void {
        if (confirm('Are you sure you want to decommission this server? All associated metrics and alerts will be permanently removed.')) {
            this.serverService.deleteServer(id).subscribe({
                next: () => {
                    this.snackBar.open('Server successfully decommissioned', 'Dismiss', { duration: 4000 });
                    this.loadServers();
                },
                error: (err) => {
                    console.error('Delete error:', err);
                    this.snackBar.open('Failed to decommission server. Higher level permissions might be required.', 'Dismiss', { duration: 5000 });
                }
            });
        }
    }
}
