import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoginComponent } from './login.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { AuthService } from '../../../core/services/auth.service';
import { of, throwError } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';

describe('LoginComponent', () => {
    let component: LoginComponent;
    let fixture: ComponentFixture<LoginComponent>;
    let authService: jasmine.SpyObj<AuthService>;

    beforeEach(async () => {
        const spy = jasmine.createSpyObj('AuthService', ['login']);

        await TestBed.configureTestingModule({
            imports: [
                LoginComponent,
                HttpClientTestingModule,
                RouterTestingModule,
                ReactiveFormsModule,
                NoopAnimationsModule
            ],
            providers: [
                { provide: AuthService, useValue: spy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(LoginComponent);
        component = fixture.componentInstance;
        authService = TestBed.get(AuthService);
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should show error when login fails', () => {
        authService.login.and.returnValue(throwError({ message: 'Invalid credentials' }));

        component.loginForm.setValue({ email: 'test@demo.com', password: 'password123' });
        component.onSubmit();

        expect(component.error).toBe('Invalid credentials');
        expect(component.loading).toBeFalse();
    });

    it('should be invalid when empty', () => {
        expect(component.loginForm.valid).toBeFalse();
    });
});
