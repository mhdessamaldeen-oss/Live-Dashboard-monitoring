import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RoleService, RoleDto, CreateRoleRequest, UpdateRolePermissionsRequest, PermissionDto } from '../../../core/services/role.service';
import { AppTableComponent, AppTableColumn } from '../../../shared/components/app-table/app-table.component';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RoleDialog } from '../role-dialog/role-dialog';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [
    CommonModule,
    AppTableComponent,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule,
    MatChipsModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './role-list.html',
  styleUrl: './role-list.css'
})
export class RoleList implements OnInit {
  private roleService = inject(RoleService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);
  public authService = inject(AuthService);

  roles: RoleDto[] = [];
  loading = false;

  columns: AppTableColumn[] = [
    { key: 'name', label: 'Role Name', type: 'template' },
    { key: 'description', label: 'Description', type: 'template' },
    { key: 'permissions', label: 'Permissions', type: 'template' },
    { key: 'actions', label: 'Actions', type: 'template', style: { 'text-align': 'right', 'width': '80px' } }
  ];

  ngOnInit() {
    this.loadRoles();
  }

  loadRoles() {
    this.loading = true;
    this.roleService.getRoles().subscribe({
      next: (roles) => {
        this.roles = roles;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  openRoleDialog(role?: RoleDto) {
    const dialogRef = this.dialog.open(RoleDialog, {
      width: '1000px',
      maxWidth: '95vw',
      data: { role } // Pass role for editing, or undefined for creating
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        if (role) {
          // Update logic would go here if backend supports updating role details (name/desc)
          // For now, we mainly update permissions via a separate call if needed, 
          // but lets assume the dialog handles calling the service for simplicity or returns the data to call here.
          // Based on our backend, we have UpdateRolePermissions.
          const updateRequest: UpdateRolePermissionsRequest = {
            roleId: role.id,
            permissionIds: result.permissionIds
          };
          this.roleService.updateRolePermissions(updateRequest).subscribe(() => {
            this.snackBar.open('Role permissions updated', 'Dismiss', { duration: 3000 });
            this.loadRoles();
          });

        } else {
          const createRequest: CreateRoleRequest = {
            name: result.name,
            description: result.description,
            permissionIds: result.permissionIds
          };
          this.roleService.createRole(createRequest).subscribe(() => {
            this.snackBar.open('Role created successfully', 'Dismiss', { duration: 3000 });
            this.loadRoles();
          });
        }
      }
    });
  }

  deleteRole(role: RoleDto) {
    if (confirm(`Are you sure you want to delete role '${role.name}'?`)) {
      this.roleService.deleteRole(role.id).subscribe({
        next: () => {
          this.snackBar.open('Role deleted successfully', 'Dismiss', { duration: 3000 });
          this.loadRoles();
        },
        error: (err) => {
          this.snackBar.open(err.message || 'Failed to delete role', 'Dismiss', { duration: 3000 });
        }
      });
    }
  }
}
