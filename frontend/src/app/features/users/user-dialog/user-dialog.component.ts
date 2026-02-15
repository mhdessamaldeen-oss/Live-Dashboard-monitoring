import { Component, inject, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UserService, CreateUserRequest, UpdateUserRequest } from '../../../core/services/user.service';
import { RoleService, RoleDto } from '../../../core/services/role.service';
import { UserDto } from '../../../core/models/auth.models';
import { MatCheckboxModule } from '@angular/material/checkbox';

@Component({
  selector: 'app-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatCheckboxModule
  ],
  templateUrl: './user-dialog.component.html',
  styleUrl: './user-dialog.component.css'
})
export class UserDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private userService = inject(UserService);
  private roleService = inject(RoleService);
  private dialogRef = inject(MatDialogRef<UserDialogComponent>);

  loading = false;
  errorMessage: string | null = null;
  roles: RoleDto[] = [];
  isEditMode = false;
  currentUser: UserDto | undefined;

  userForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: [''],
    role: ['', Validators.required],
    isActive: [true]
  });

  constructor(@Inject(MAT_DIALOG_DATA) public data: { user?: UserDto }) {
    if (data && data.user) {
      this.isEditMode = true;
      this.currentUser = data.user;
      this.userForm.patchValue({
        firstName: data.user.firstName,
        lastName: data.user.lastName,
        email: data.user.email,
        role: data.user.role,
        isActive: data.user.isActive
      });
      // Remove password validation in edit mode
      this.userForm.get('password')?.clearValidators();
      this.userForm.get('password')?.updateValueAndValidity();
    } else {
      // Add password validation in create mode
      this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
      this.userForm.get('password')?.updateValueAndValidity();
    }
  }

  ngOnInit() {
    this.loadRoles();
  }

  loadRoles() {
    this.roleService.getRoles().subscribe(roles => {
      this.roles = roles;
      // If fetching roles fails, we might want to fallback or show error, but for now assuming success or empty list
    });
  }

  onSubmit(): void {
    if (this.userForm.valid) {
      this.loading = true;
      this.errorMessage = null;

      if (this.isEditMode && this.currentUser) {
        const request: UpdateUserRequest = {
          id: this.currentUser.id,
          firstName: this.userForm.get('firstName')?.value || '',
          lastName: this.userForm.get('lastName')?.value || '',
          email: this.userForm.get('email')?.value || '',
          role: this.userForm.get('role')?.value || '',
          isActive: this.userForm.get('isActive')?.value || false
        };

        this.userService.updateUser(this.currentUser.id, request).subscribe({
          next: (user: UserDto) => {
            this.loading = false;
            this.dialogRef.close(user);
          },
          error: (err) => this.handleError(err)
        });
      } else {
        const request: CreateUserRequest = {
          firstName: this.userForm.get('firstName')?.value || '',
          lastName: this.userForm.get('lastName')?.value || '',
          email: this.userForm.get('email')?.value || '',
          role: this.userForm.get('role')?.value || '',
          password: this.userForm.get('password')?.value || ''
        };

        this.userService.createUser(request).subscribe({
          next: (user: UserDto) => {
            this.loading = false;
            this.dialogRef.close(user);
          },
          error: (err) => this.handleError(err)
        });
      }
    }
  }

  handleError(err: any) {
    this.loading = false;
    console.error('User operation error:', err);

    if (err.error?.errors?.length > 0) {
      this.errorMessage = err.error.errors[0];
    } else if (err.error?.message) {
      this.errorMessage = err.error.message;
    } else if (err.status === 403) {
      this.errorMessage = 'Access Denied: You do not have permission.';
    } else {
      this.errorMessage = err.message || 'Operation failed. Please try again.';
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
