import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DashboardHomeComponent } from './dashboard-home.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideMockStore } from '@ngrx/store/testing';
import { SignalRService } from '../../../core/services/signalr.service';
import { of } from 'rxjs';

describe('DashboardHomeComponent', () => {
    let component: DashboardHomeComponent;
    let fixture: ComponentFixture<DashboardHomeComponent>;
    let mockSignalRService: any;

    beforeEach(async () => {
        mockSignalRService = {
            metrics$: of(null),
            alertTriggered$: of(null),
            alertResolved$: of(null),
            connectionStatus$: of('Connected'),
            presence$: of(1)
        };

        await TestBed.configureTestingModule({
            imports: [
                DashboardHomeComponent,
                HttpClientTestingModule,
                NoopAnimationsModule
            ],
            providers: [
                provideMockStore({}),
                { provide: SignalRService, useValue: mockSignalRService }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(DashboardHomeComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should show live metrics panel title', () => {
        const compiled = fixture.nativeElement as HTMLElement;
        expect(compiled.querySelector('.panel-title')?.textContent).toContain('Live Metrics');
    });
});
