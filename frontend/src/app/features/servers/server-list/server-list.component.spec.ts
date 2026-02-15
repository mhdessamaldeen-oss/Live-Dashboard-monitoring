import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ServerListComponent } from './server-list.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideMockStore } from '@ngrx/store/testing';
import { RouterTestingModule } from '@angular/router/testing';

describe('ServerListComponent', () => {
    let component: ServerListComponent;
    let fixture: ComponentFixture<ServerListComponent>;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [
                ServerListComponent,
                HttpClientTestingModule,
                RouterTestingModule,
                NoopAnimationsModule
            ],
            providers: [
                provideMockStore({ initialState: { servers: { servers: [], loading: false } } })
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(ServerListComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should display page title', () => {
        const compiled = fixture.nativeElement as HTMLElement;
        expect(compiled.querySelector('.page-title')?.textContent).toContain('Infrastructure');
    });
});
