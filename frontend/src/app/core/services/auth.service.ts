import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, UserDto, Roles } from '../models/auth.models';
import { Result } from '../models/result.models';
import { jwtDecode } from 'jwt-decode';
import { map } from 'rxjs/operators';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private readonly TOKEN_KEY = 'auth_token';
    private readonly REFRESH_TOKEN_KEY = 'refresh_token';
    private readonly USER_KEY = 'auth_user';

    private currentUserSubject = new BehaviorSubject<UserDto | null>(this.getUserFromStorage());
    public currentUser$ = this.currentUserSubject.asObservable();

    constructor(private http: HttpClient, private router: Router) { }

    public get currentUserValue(): UserDto | null {
        return this.currentUserSubject.value;
    }

    public get token(): string | null {
        return localStorage.getItem(this.TOKEN_KEY);
    }

    login(credentials: LoginRequest): Observable<AuthResponse> {
        return this.http.post<Result<AuthResponse>>(`${environment.apiUrl}/Auth/login`, credentials).pipe(
            map((response: Result<AuthResponse>) => {
                if (response.isSuccess) {
                    this.setSession(response.data);
                    return response.data;
                }
                throw new Error(response.errors?.[0] || 'Login failed');
            })
        );
    }

    logout(): void {
        localStorage.removeItem(this.TOKEN_KEY);
        localStorage.removeItem(this.REFRESH_TOKEN_KEY);
        localStorage.removeItem(this.USER_KEY);
        this.currentUserSubject.next(null);
        this.router.navigate(['/auth/login']);
    }

    private setSession(authResult: AuthResponse): void {
        localStorage.setItem(this.TOKEN_KEY, authResult.token);
        localStorage.setItem(this.REFRESH_TOKEN_KEY, authResult.refreshToken);
        localStorage.setItem(this.USER_KEY, JSON.stringify(authResult.user));
        this.currentUserSubject.next(authResult.user);
    }

    private getUserFromStorage(): UserDto | null {
        try {
            const userJson = localStorage.getItem(this.USER_KEY);
            if (!userJson || userJson === 'undefined') return null;
            return JSON.parse(userJson);
        } catch (e) {
            localStorage.removeItem(this.USER_KEY);
            return null;
        }
    }

    isLoggedIn(): boolean {
        const token = this.token;
        if (!token) return false;

        try {
            const decoded: any = jwtDecode(token);
            // Check if token is expired or expiring soon (within 30 seconds)
            const isExpired = decoded.exp * 1000 < Date.now() + 30000;
            return !isExpired;
        } catch {
            return false;
        }
    }

    refreshToken(): Observable<AuthResponse> {
        const token = localStorage.getItem(this.TOKEN_KEY);
        const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);

        return this.http.post<Result<AuthResponse>>(`${environment.apiUrl}/Auth/refresh`, {
            token,
            refreshToken
        }).pipe(
            map((response: Result<AuthResponse>) => {
                if (response.isSuccess) {
                    this.setSession(response.data);
                    return response.data;
                }
                this.logout();
                throw new Error(response.errors?.[0] || 'Refresh failed');
            })
        );
    }

    isAdmin(): boolean {
        return this.currentUserValue?.role === Roles.Admin;
    }
}
