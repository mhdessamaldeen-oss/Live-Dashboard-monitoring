import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Result } from '../models/result.models';

export interface PermissionDto {
    id: number;
    name: string;
    description: string;
    isAssigned?: boolean;
}

export interface RoleDto {
    id: number;
    name: string;
    description: string;
    permissions: PermissionDto[];
}

export interface CreateRoleRequest {
    name: string;
    description: string;
    permissionIds: number[];
}

export interface UpdateRolePermissionsRequest {
    roleId: number;
    permissionIds: number[];
}

@Injectable({
    providedIn: 'root'
})
export class RoleService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/roles`;

    getRoles(): Observable<RoleDto[]> {
        return this.http.get<Result<RoleDto[]>>(this.apiUrl).pipe(
            map(res => res.data)
        );
    }

    getAllPermissions(): Observable<PermissionDto[]> {
        return this.http.get<Result<PermissionDto[]>>(`${this.apiUrl}/permissions`).pipe(
            map(res => res.data)
        );
    }

    createRole(request: CreateRoleRequest): Observable<number> {
        return this.http.post<Result<number>>(this.apiUrl, request).pipe(
            map(res => {
                if (res.isSuccess) {
                    return res.data;
                }
                throw new Error(res.errors?.[0] || 'Failed to create role');
            })
        );
    }

    updateRolePermissions(request: UpdateRolePermissionsRequest): Observable<number> {
        return this.http.put<Result<number>>(`${this.apiUrl}/${request.roleId}/permissions`, request).pipe(
            map(res => {
                if (res.isSuccess) {
                    return res.data;
                }
                throw new Error(res.errors?.[0] || 'Failed to update role permissions');
            })
        );
    }

    deleteRole(id: number): Observable<void> {
        return this.http.delete<Result<void>>(`${this.apiUrl}/${id}`).pipe(
            map(res => {
                if (!res.isSuccess) {
                    throw new Error(res.errors?.[0] || 'Failed to delete role');
                }
            })
        );
    }
}
