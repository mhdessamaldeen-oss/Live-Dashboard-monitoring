import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UserDto } from '../models/auth.models';
import { Result } from '../models/result.models';

export interface CreateUserRequest {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    role: string;
}

export interface UpdateUserRequest {
    id: number;
    email: string;
    firstName: string;
    lastName: string;
    role: string; // The backend expects role Name (e.g. "Admin")
    isActive: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/users`;

    getUsers(): Observable<UserDto[]> {
        return this.http.get<Result<UserDto[]>>(this.apiUrl).pipe(
            map(res => res.data)
        );
    }

    createUser(request: CreateUserRequest): Observable<UserDto> {
        return this.http.post<Result<UserDto>>(this.apiUrl, request).pipe(
            map(res => {
                if (res.isSuccess) {
                    return res.data;
                }
                throw new Error(res.errors?.[0] || 'Failed to create user');
            })
        );
    }

    updateUser(id: number, request: UpdateUserRequest): Observable<UserDto> {
        return this.http.put<Result<UserDto>>(`${this.apiUrl}/${id}`, request).pipe(
            map(res => {
                if (res.isSuccess) {
                    return res.data;
                }
                throw new Error(res.errors?.[0] || 'Failed to update user');
            })
        );
    }

    deleteUser(id: number): Observable<void> {
        return this.http.delete<Result<void>>(`${this.apiUrl}/${id}`).pipe(
            map(res => {
                if (!res.isSuccess) {
                    throw new Error(res.errors?.[0] || 'Failed to delete user');
                }
            })
        );
    }
}
