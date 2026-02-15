export interface UserDto {
    id: number;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
    isActive: boolean;
}

export interface AuthResponse {
    token: string;
    refreshToken: string;
    expiration: string;
    user: UserDto;
}

export interface LoginRequest {
    email: string;
    password: string;
}

export const Roles = {
    Admin: 'Admin',
    Operator: 'Operator',
    Viewer: 'Viewer'
} as const;
