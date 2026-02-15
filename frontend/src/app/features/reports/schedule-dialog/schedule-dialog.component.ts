import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ServerDto } from '../../../core/models/server.models';

@Component({
    selector: 'app-schedule-dialog',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatButtonModule,
        MatIconModule
    ],
    template: `
    <h2 mat-dialog-title class="gradient-text">New Automated Schedule</h2>
    <mat-dialog-content>
      <form [formGroup]="scheduleForm" class="dialog-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Schedule Name</mat-label>
          <input matInput formControlName="name" placeholder="e.g., Weekly Node Audit">
          <mat-icon matPrefix color="primary">label</mat-icon>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Target Infrastructure</mat-label>
          <mat-select formControlName="serverId">
            <mat-option *ngFor="let server of servers" [value]="server.id">
              {{ server.name }} ({{ server.ipAddress }})
            </mat-option>
          </mat-select>
          <mat-icon matPrefix color="primary">dns</mat-icon>
        </mat-form-field>

        <div class="row">
          <mat-form-field appearance="outline">
            <mat-label>Report Type</mat-label>
            <mat-select formControlName="reportType">
              <mat-option value="Summary">Summary</mat-option>
              <mat-option value="Performance">Performance</mat-option>
              <mat-option value="Security">Security</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Recipients (CSV Emails)</mat-label>
            <input matInput formControlName="recipients" placeholder="ops@company.com">
            <mat-icon matPrefix color="primary">mail</mat-icon>
          </mat-form-field>
        </div>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Cron Expression (Schedule)</mat-label>
          <input matInput formControlName="cronExpression" placeholder="0 9 * * * (Daily at 9 AM)">
          <mat-icon matPrefix color="primary">schedule</mat-icon>
          <mat-hint>Use standard Crontab format (Min Hour Day Month DayOfWeek)</mat-hint>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description" rows="2"></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-flat-button color="primary" [disabled]="scheduleForm.invalid" (click)="onSubmit()">
        Create Schedule
      </button>
    </mat-dialog-actions>
  `,
    styles: [`
    .dialog-form {
      display: flex;
      flex-direction: column;
      gap: 8px;
      padding-top: 8px;
      min-width: 450px;
    }
    .full-width { width: 100%; }
    .row {
      display: flex;
      gap: 16px;
    }
    .row mat-form-field { flex: 1; }
    .gradient-text {
      background: linear-gradient(135deg, var(--primary), #8b5cf6);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      font-weight: 800;
    }
  `]
})
export class ScheduleDialogComponent {
    scheduleForm: FormGroup;

    constructor(
        private fb: FormBuilder,
        private dialogRef: MatDialogRef<ScheduleDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public servers: ServerDto[]
    ) {
        this.scheduleForm = this.fb.group({
            name: ['', Validators.required],
            description: [''],
            cronExpression: ['0 9 * * *', Validators.required],
            recipients: ['', [Validators.required]],
            reportType: ['Summary', Validators.required],
            serverId: [null, Validators.required]
        });
    }

    onCancel(): void {
        this.dialogRef.close();
    }

    onSubmit(): void {
        if (this.scheduleForm.valid) {
            this.dialogRef.close(this.scheduleForm.value);
        }
    }
}
