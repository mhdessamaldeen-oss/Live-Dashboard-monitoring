import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ServerService } from '../../../core/services/server.service';
import { ServerDto } from '../../../core/models/server.models';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-provision-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatTabsModule,
    FormsModule,
    ReactiveFormsModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './provision-dialog.component.html',
  styleUrl: './provision-dialog.component.css'
})
export class ProvisionDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private serverService = inject(ServerService);
  private dialogRef = inject(MatDialogRef<ProvisionDialogComponent>);
  public data = inject<ServerDto | null>(MAT_DIALOG_DATA);

  loading = false;
  isEdit = false;
  errorMessage: string | null = null;

  provisionForm = this.fb.group({
    id: [0],
    name: ['', Validators.required],
    ipAddress: ['127.0.0.1'],
    hostName: ['internal-node'],
    operatingSystem: ['Ubuntu 22.04 LTS'],
    location: ['Local Data Center'],
    status: ['Online'],
    isActive: [true],
    cpuWarningThreshold: [70, [Validators.required, Validators.min(1), Validators.max(100)]],
    cpuCriticalThreshold: [90, [Validators.required, Validators.min(1), Validators.max(100)]],
    memoryWarningThreshold: [70, [Validators.required, Validators.min(1), Validators.max(100)]],
    memoryCriticalThreshold: [90, [Validators.required, Validators.min(1), Validators.max(100)]],
    diskWarningThreshold: [80, [Validators.required, Validators.min(1), Validators.max(100)]],
    diskCriticalThreshold: [95, [Validators.required, Validators.min(1), Validators.max(100)]]
  });

  ngOnInit(): void {
    if (this.data) {
      this.isEdit = true;
      this.provisionForm.patchValue(this.data);
    }
  }

  onProvision(): void {
    if (this.provisionForm.valid) {
      this.loading = true;
      this.errorMessage = null;

      const serverData = {
        ...this.provisionForm.value,
        hostName: this.provisionForm.value.hostName || this.provisionForm.value.name?.toLowerCase() + '.internal'
      };

      const request = this.isEdit
        ? this.serverService.updateServer(this.data!.id, serverData as any)
        : this.serverService.createServer(serverData as any);

      request.subscribe({
        next: (server: ServerDto) => {
          this.loading = false;
          this.dialogRef.close(server);
        },
        error: (err) => {
          this.loading = false;
          console.error('Save error:', err);

          if (err.error?.errors?.length > 0) {
            this.errorMessage = err.error.errors[0];
          } else if (err.error?.message) {
            this.errorMessage = err.error.message;
          } else if (err.status === 403) {
            this.errorMessage = 'Access Denied: Administrative clearance is required for this action.';
          } else {
            this.errorMessage = err.message || 'Action failed. System reported an internal error. Please try again later.';
          }
        }
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
