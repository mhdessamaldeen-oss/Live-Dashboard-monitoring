import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
    let service: AuthService;
    let httpMock: HttpTestingController;
    let routerSpy: jasmine.SpyObj<Router>;

    beforeEach(() => {
        routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                AuthService,
                { provide: Router, useValue: routerSpy }
            ]
        });

        service = TestBed.inject(AuthService);
        httpMock = TestBed.inject(HttpTestingController);

        // Clear localStorage
        localStorage.clear();
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should login successfully', () => {
        const mockAuthResponse = {
            token: 'mock-token',
            refreshToken: 'mock-refresh',
            expiration: new Date().toISOString(),
            user: {
                id: 1,
                email: 'admin@demo.com',
                role: 'Admin',
                firstName: 'Admin',
                lastName: 'User',
                isActive: true
            }
        };

        const mockResult = {
            isSuccess: true,
            data: mockAuthResponse
        };

        service.login({ email: 'test@test.com', password: 'password' }).subscribe(res => {
            expect(res).toEqual(mockAuthResponse);
            expect(localStorage.getItem('auth_token')).toBe('mock-token');
            expect(service.currentUserValue?.role).toBe('Admin');
        });

        const req = httpMock.expectOne(`${environment.apiUrl}/Auth/login`);
        expect(req.request.method).toBe('POST');
        req.flush(mockResult);
    });

    it('should logout and navigate to login', () => {
        localStorage.setItem('auth_token', 'token');
        service.logout();
        expect(localStorage.getItem('auth_token')).toBeNull();
        expect(service.currentUserValue).toBeNull();
        expect(routerSpy.navigate).toHaveBeenCalledWith(['/auth/login']);
    });
});
