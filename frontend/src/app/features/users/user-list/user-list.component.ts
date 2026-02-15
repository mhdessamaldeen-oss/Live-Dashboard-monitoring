import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { UserDto } from '../../../core/models/auth.models';
import { AppTableComponent, AppTableColumn } from '../../../shared/components/app-table/app-table.component';
import { UserDialogComponent } from '../user-dialog/user-dialog.component';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule,
    AppTableComponent,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.css'
})
export class UserListComponent implements OnInit {
  private userService = inject(UserService);
  public authService = inject(AuthService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  users: UserDto[] = [];
  loading = false;

  columns: AppTableColumn[] = [
    { key: 'name', label: 'Identity', type: 'template' },
    { key: 'role', label: 'Authorization', type: 'template' },
    { key: 'status', label: 'Account Status', type: 'template' },
    { key: 'actions', label: 'Management', type: 'template', style: { 'text-align': 'right' } }
  ];

  ngOnInit(): void {
    if (!this.authService.isAdmin()) {
      this.columns = this.columns.filter(c => c.key !== 'actions');
    }
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.userService.getUsers().subscribe({
      next: (data) => {
        this.users = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  openUserDialog(user?: UserDto): void {
    const dialogRef = this.dialog.open(UserDialogComponent, {
      width: '560px',
      disableClose: true,
      data: { user }
    });

    dialogRef.afterClosed().subscribe((result: UserDto | null) => {
      if (result) {
        const action = user ? 'updated' : 'created';
        this.snackBar.open(`User ${result.firstName} ${result.lastName} ${action} successfully`, 'Dismiss', {
          duration: 5000,
          panelClass: ['success-snack']
        });
        this.loadUsers();
      }
    });
  }

  provisionUser(): void {
    this.openUserDialog();
  }

  deleteUser(user: UserDto): void {
    if (confirm(`Are you sure you want to delete user ${user.firstName} ${user.lastName}? This action cannot be undone.`)) {
      this.userService.deleteUser(user.id).subscribe({
        next: () => {
          this.snackBar.open('User deleted successfully', 'Dismiss', { duration: 3000 });
          this.loadUsers();
        },
        error: (err) => {
          this.snackBar.open(err.message || 'Failed to delete user', 'Dismiss', { duration: 3000 });
        }
      });
    }
  }
}
