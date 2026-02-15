import { Component, inject, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule } from '@angular/material/expansion';
import { RoleService, PermissionDto, RoleDto } from '../../../core/services/role.service';

interface PermissionGroup {
  name: string;
  permissions: PermissionDto[];
  isSelected: boolean;
  isIndeterminate: boolean;
}

@Component({
  selector: 'app-role-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatExpansionModule
  ],
  templateUrl: './role-dialog.html',
  styleUrl: './role-dialog.css'
})
export class RoleDialog implements OnInit {
  private fb = inject(FormBuilder);
  private roleService = inject(RoleService);
  private dialogRef = inject(MatDialogRef<RoleDialog>);

  // Use explicit injection for MAT_DIALOG_DATA
  isEditMode = false;
  permissionGroups: PermissionGroup[] = [];
  selectedPermissionIds: Set<number> = new Set();
  loading = false;

  roleForm = this.fb.group({
    name: ['', Validators.required],
    description: ['']
  });

  constructor(@Inject(MAT_DIALOG_DATA) public data: { role?: RoleDto }) {
    this.isEditMode = !!data.role;
    if (this.isEditMode && data.role) {
      this.roleForm.patchValue({
        name: data.role.name,
        description: data.role.description
      });
      // In edit mode, name/description might be read-only if backend doesn't support updating them
      this.roleForm.get('name')?.disable();
      this.roleForm.get('description')?.disable();

      data.role.permissions.forEach(p => this.selectedPermissionIds.add(p.id));
    }
  }

  ngOnInit() {
    this.loading = true;
    this.roleService.getAllPermissions().subscribe(perms => {
      this.groupPermissions(perms);
      this.loading = false;
    });
  }

  groupPermissions(perms: PermissionDto[]) {
    const groupsMap = new Map<string, PermissionDto[]>();

    perms.forEach(p => {
      const parts = p.name.split('.');
      const category = parts.length > 2 ? parts[1] : 'General';

      if (!groupsMap.has(category)) {
        groupsMap.set(category, []);
      }
      groupsMap.get(category)?.push(p);
    });

    this.permissionGroups = Array.from(groupsMap.entries()).map(([name, permissions]) => {
      const group = {
        name,
        permissions,
        isSelected: false,
        isIndeterminate: false
      };
      this.updateGroupState(group);
      return group;
    });
  }

  getShortName(fullName: string): string {
    const parts = fullName.split('.');
    return parts[parts.length - 1];
  }

  isPermissionSelected(id: number): boolean {
    return this.selectedPermissionIds.has(id);
  }

  togglePermission(id: number) {
    if (this.selectedPermissionIds.has(id)) {
      this.selectedPermissionIds.delete(id);
    } else {
      this.selectedPermissionIds.add(id);
    }
    this.permissionGroups.forEach(g => this.updateGroupState(g));
  }

  toggleGroup(group: PermissionGroup) {
    const targetState = !group.isSelected;
    group.permissions.forEach(p => {
      if (targetState) {
        this.selectedPermissionIds.add(p.id);
      } else {
        this.selectedPermissionIds.delete(p.id);
      }
    });
    this.updateGroupState(group);
  }

  updateGroupState(group: PermissionGroup) {
    const selectedInGroup = group.permissions.filter(p => this.selectedPermissionIds.has(p.id)).length;
    group.isSelected = selectedInGroup === group.permissions.length;
    group.isIndeterminate = selectedInGroup > 0 && selectedInGroup < group.permissions.length;
  }

  getSelectedCount(group: PermissionGroup): number {
    return group.permissions.filter(p => this.selectedPermissionIds.has(p.id)).length;
  }

  onSubmit() {
    if (this.roleForm.invalid) return;

    const result = {
      name: this.roleForm.get('name')?.getRawValue(),
      description: this.roleForm.get('description')?.getRawValue(),
      permissionIds: Array.from(this.selectedPermissionIds)
    };

    this.dialogRef.close(result);
  }
}
