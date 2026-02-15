import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StatCardComponent } from './stat-card.component';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

describe('StatCardComponent', () => {
    let component: StatCardComponent;
    let fixture: ComponentFixture<StatCardComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [StatCardComponent, MatCardModule, MatIconModule],
        }).compileComponents();

        fixture = TestBed.createComponent(StatCardComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should display the correct label and value', () => {
        component.label = 'Total Servers';
        component.value = '10';
        fixture.detectChanges();

        const compiled = fixture.nativeElement as HTMLElement;
        expect(compiled.querySelector('.stat-label')?.textContent).toContain('Total Servers');
        expect(compiled.querySelector('.stat-value')?.textContent).toContain('10');
    });

    it('should display the correct icon', () => {
        component.icon = 'dns';
        fixture.detectChanges();

        const iconElement = fixture.nativeElement.querySelector('mat-icon');
        expect(iconElement.textContent.trim()).toBe('dns');
    });
});
