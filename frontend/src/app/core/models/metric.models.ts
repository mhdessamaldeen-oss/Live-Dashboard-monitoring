export interface MetricDto {
    id: number;
    serverId: number;
    cpuUsage: number;
    memoryUsage: number;
    diskUsage: number;
    diskUsagePercent?: number;
    networkIn: number;
    networkOut: number;
    timestamp: string;
    disks?: DiskDto[];
}

export interface DiskDto {
    driveLetter: string;
    freeSpaceMB: number;
    totalSpaceMB: number;
    usedPercentage: number;
}

export interface ServerDetailsDto {
    id: number;
    name: string;
    hostName: string;
    ipAddress: string;
    operatingSystem: string;
    location: string;
    status: string;
    isActive: boolean;
    latestMetrics: MetricDto[];
}
