import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-stat-card',
    standalone: true,
    imports: [CommonModule, MatCardModule, MatIconModule],
    templateUrl: './stat-card.component.html',
    styleUrls: ['./stat-card.component.css']
})
export class StatCardComponent {
    @Input() label = '';
    @Input() value = '';
    @Input() icon = '';
    @Input() iconBg = 'var(--primary-soft)';
    @Input() iconColor = 'var(--primary)';
}
