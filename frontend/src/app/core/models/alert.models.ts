export enum AlertStatus {
    Active = 'Active',
    Acknowledged = 'Acknowledged',
    Resolved = 'Resolved',
    Expired = 'Expired'
}

export enum AlertSeverity {
    Info = 'Info',
    Warning = 'Warning',
    Critical = 'Critical'
}

export interface AlertDto {
    id: number;
    serverId: number;
    serverName: string;
    title: string;
    message: string;
    status: AlertStatus;
    severity: AlertSeverity;
    metricType: string;
    metricValue: number;
    thresholdValue: number;
    createdAt: Date;
    resolvedAt?: Date;
    resolvedByUserId?: number;
}

export interface AlertSummaryDto {
    totalAlerts: number;
    activeAlerts: number;
    criticalAlerts: number;
    warningAlerts: number;
    infoAlerts: number;
}
