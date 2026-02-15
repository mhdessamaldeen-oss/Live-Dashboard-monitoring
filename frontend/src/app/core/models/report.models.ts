export interface ReportDto {
    id: number;
    serverId: number;
    serverName: string;
    title: string;
    description?: string;
    status: 'Pending' | 'Processing' | 'Completed' | 'Failed';
    fileName?: string;
    fileSizeBytes?: number;
    createdAt: string;
    completedAt?: string;
}

export interface CreateReportRequest {
    serverId: number;
    title: string;
    description?: string;
    dateRangeStart?: string;
    dateRangeEnd?: string;
}

export interface ReportTemplateDto {
    id: number;
    name: string;
    description: string;
    category: 'performance' | 'security' | 'summary' | 'compliance' | 'custom' | 'financial';
    features: string[];
    parameters: any;
}

export interface ScheduledReportDto {
    id: number;
    name: string;
    description?: string;
    cronExpression: string;
    lastRunAt?: string;
    nextRunAt?: string;
    isActive: boolean;
    recipients: string;
    reportType: string;
    serverId: number;
    serverName?: string;
}

export interface CreateReportScheduleRequest {
    name: string;
    description?: string;
    cronExpression: string;
    recipients: string;
    reportType: string;
    serverId: number;
}
