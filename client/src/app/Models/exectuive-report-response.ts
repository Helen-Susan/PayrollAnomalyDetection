import { DashboardSummary } from './dashboard-summary';

export interface ExecutiveReportResponseDto {

  executiveSummary: string;

  payrollHealthScore: number;

  riskLevel: string;

  keyInsights: string[];

  recommendations: string[];

  conclusion: string;

  dashboardSummary: DashboardSummary;
}
