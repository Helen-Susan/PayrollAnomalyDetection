import { CycleStatus } from '../types/upload';

export interface PayrollCycle {
  id: string;
  fileName: string;
  fileType:  'csv' ;
  status: CycleStatus;
  uploadedAt: Date;
  summary?: CycleSummary;
}

export interface CycleSummary {
  cycleId: string;
  totalEmployees: number;
  employeesPaid: number;
  pendingApprovals: number;
  emptyCalculations: number;
  duplicateEmployees: number;
  negativeSalaries: number;
  highSalaryAnomalies: number;
}

export interface SummaryBreakdown {
  category: string;
  count: number;
  amount: number;
}

export interface CycleStatusResponse {
  cycleId: string;
  status: CycleStatus;
  updatedAt: Date;
  message?: string;
}
