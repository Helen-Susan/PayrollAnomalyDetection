export interface CycleTrend {
  cycleId: string;
  anomalyCount: number;
}

export interface DashboardSummary {
  totalCycles: number;
  totalEmployees: number;
  totalEmployeesPaid: number;
  totalPendingApprovals: number;
  totalAnomalies: number;
  cycleTrend: CycleTrend[];
}
