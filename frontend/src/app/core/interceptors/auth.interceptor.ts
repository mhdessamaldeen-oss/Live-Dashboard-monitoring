import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const token = authService.token;

    if (token) {
        req = req.clone({
            setHeaders: {
                Authorization: `Bearer ${token}`
            }
        });
    }

    return next(req).pipe(
        catchError((error: any) => {
            if (error instanceof HttpErrorResponse && error.status === 401) {
                // Token might be expired, try to refresh
                return authService.refreshToken().pipe(
                    switchMap((response) => {
                        // Refresh successful, retry the request with new token
                        const newRequest = req.clone({
                            setHeaders: {
                                Authorization: `Bearer ${response.token}`
                            }
                        });
                        return next(newRequest);
                    }),
                    catchError((refreshError) => {
                        // Refresh failed, logout
                        authService.logout();
                        return throwError(() => refreshError);
                    })
                );
            }
            return throwError(() => error);
        })
    );
};
