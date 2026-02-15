export interface ServerDto {
    id: number;
    name: string;
    hostName: string;
    ipAddress: string;
    operatingSystem: string;
    location: string;
    status: string;
    isActive: boolean;
    isHost: boolean;
    cpuUsage?: number;
    memoryUsage?: number;
    diskUsage?: number;
    lastUpdate?: Date;
    cpuWarningThreshold: number;
    cpuCriticalThreshold: number;
    memoryWarningThreshold: number;
    memoryCriticalThreshold: number;
    diskWarningThreshold: number;
    diskCriticalThreshold: number;
}

export interface PagedResult<T> {
    items: T[];
    page: number;
    totalPages: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

// Dashboard specific models
export interface SystemHealthDto {
    score: number;
    status: 'healthy' | 'warning' | 'critical';
}

export interface KeyMetricDto {
    label: string;
    value: string;
    icon: string;
    trend: 'up' | 'down' | 'stable';
    change: string;
    changeType: 'up' | 'down' | 'stable';
}

export interface ActivityDto {
    type: 'alert' | 'system' | 'user';
    message: string;
    timestamp: Date;
    actionable: boolean;
    actionText?: string;
}
