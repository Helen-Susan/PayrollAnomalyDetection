import { Component, OnInit } from '@angular/core';
import { ReportService } from '../../../core/services/report-service';
import { DashboardSummary } from '../../Models/dashboard-summary';
import { ExecutiveReportResponseDto } from '../../Models/exectuive-report-response';
import { DepartmentRiskScoreDto } from '../../Models/department-risk-score';
import { ChartOptions } from 'chart.js';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { SidebarComponent } from '../shared/sidenavcomponent/sidenavcomponent';
import { ChangeDetectorRef } from '@angular/core';

import { Chart, CategoryScale, LinearScale, BarElement, LineElement, PointElement, ArcElement, Tooltip, Legend } from 'chart.js';
import { registerables } from 'chart.js';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import { firstValueFrom } from 'rxjs';

Chart.register(...registerables);
@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css'],
  standalone: true,
  imports: [
    CommonModule,
    BaseChartDirective,
    SidebarComponent
  ]
})
export class DashboardComponent implements OnInit {

  summary: DashboardSummary | null = null;

  riskScores: DepartmentRiskScoreDto[] = [];
  executiveReport: any;

  isGeneratingReport = false;

  healthScore = 0;

  riskLevel = '';

  chartOptions: ChartOptions<any> = {

    responsive: true,

    maintainAspectRatio: false,

    plugins: {
      legend: {
        labels: {
          color: '#F8FAFC'
        }
      }
    }
  };

    riskScoreChartOptions: ChartOptions<'bar'> = {

      responsive: true,

      maintainAspectRatio: false,

      indexAxis: 'y',

      plugins: {
        legend: {
          display: false
        }
      },

      scales: {
        x: {
          beginAtZero: true
        }
      }
    
  };

  isLoading = false;
  employeeChartData: any = {
    labels: [],
    datasets: []
  };

  approvalChartData: any = {
    labels: [],
    datasets: []
  };

  trendChartData: any = {
    labels: [],
    datasets: []
  };

  riskScoreChartData: any = {
    labels: [],
    datasets: []
  };

  employeeChartType: 'bar' = 'bar';

  approvalChartType: 'pie' = 'pie';

  trendChartType: 'line' = 'line';

  riskChartType: 'bar' = 'bar';
 

  constructor(
    private reportService: ReportService,
    private cdr: ChangeDetectorRef
  ) { }

 

    ngOnInit(): void {


      console.log(this.employeeChartData);
      this.loadDashboard();
      

      this.loadExecutiveReport();
    
  }

  private loadDashboard(): void {

    this.isLoading = true;

    this.loadSummary();

    this.loadRiskScores();
  }

  private loadSummary(): void {

    console.log('loadSummary called');

    this.reportService
      .getDashboardSummary()
      .subscribe({

        next: (response) => {

          this.summary = response;

          this.buildEmployeeChart();
          this.buildApprovalChart();
          this.buildTrendChart();

          this.cdr.detectChanges();

          this.isLoading = false;
        },

        error: (error) => {

          console.error(
            'Failed to load dashboard summary',
            error
          );

          this.isLoading = false;
        }
      });
  }
  private loadRiskScores(): void {

    this.reportService
      .getRiskScores()
      .subscribe({

        next: (response) => {

          this.riskScores = response;

          this.buildRiskScoreChart();

          this.cdr.detectChanges();
        },

        error: (error) => {

          console.error(
            'Failed to load risk scores',
            error
          );
        }
      });
  }

  private buildEmployeeChart(): void {

    if (!this.summary) return;

    this.employeeChartData = {
      labels: [
        'Total Employees',
        'Employees Paid'
      ],
      datasets: [
        {
          label: 'Employees',
          data: [
            this.summary.totalEmployees,
            this.summary.totalEmployeesPaid
          ],
          backgroundColor: [
            '#3B82F6',
            '#22C55E'
          ]
        }
      ]
    };
  }

  private buildApprovalChart(): void {

    if (!this.summary) return;

    const approved =
      this.summary.totalEmployees -
      this.summary.totalPendingApprovals;

    this.approvalChartData = {
      labels: [
        'Approved',
        'Pending'
      ],
      datasets: [
        {
          data: [
            approved,
            this.summary.totalPendingApprovals
          ],
          backgroundColor: [
            '#22C55E',
            '#F59E0B'
          ]
        }
      ]
    };
  }

  private buildTrendChart(): void {

    if (!this.summary) return;

    this.trendChartData = {
      labels:
        this.summary.cycleTrend.map(
          x => x.cycleId
        ),

      datasets: [
        {
          label: 'Anomalies',
          data:
            this.summary.cycleTrend.map(
              x => x.anomalyCount
            ),
          borderColor: '#EF4444',
          backgroundColor: '#EF4444',
          tension: 0.4
        }
      ]
    };
  }

  private buildRiskScoreChart(): void {

    const grouped: { [key: string]: number } = {};

    this.riskScores.forEach(x => {

      grouped[x.department] =
        (grouped[x.department] || 0)
        + x.riskScore;
    });

    this.riskScoreChartData = {

      labels: Object.keys(grouped),

      datasets: [
        {
          label: 'Risk Score',

          data: Object.values(grouped),

          backgroundColor: '#8B5CF6'
        }
      ]
    };
  }

  get approvedEmployees(): number {
    
    if (!this.summary)
      return 0;

    return (
      

      this.summary.totalEmployees -
      this.summary.totalPendingApprovals
    );

  }
  loadExecutiveReport(): void {

    this.isGeneratingReport = true;

    this.reportService
      .generateExecutiveReport()
      .subscribe({

        next: (report:ExecutiveReportResponseDto) => {

          console.log(report);

          this.executiveReport = report;

          this.isGeneratingReport = false;
        },
      

        error: err => {

          console.error(err);

          this.isGeneratingReport = false;
        }
      });
  }

  get paidPercentage(): number {

    if (!this.summary)
      return 0;

    return Math.round(
      (
        this.summary.totalEmployeesPaid /
        this.summary.totalEmployees
      ) * 100
    );
  }

  async exportPdf(): Promise<void> {

    if (!this.executiveReport) {

      
try {

  this.executiveReport =
    await firstValueFrom(
      this.reportService
        .generateExecutiveReport()
    );

  this.healthScore =
    this.executiveReport.payrollHealthScore;

  this.riskLevel =
    this.executiveReport.riskLevel;

} catch (error) {

  console.error(
    'Failed to generate executive report',
    error
  );

  return;
}


    }

    const pdf =
      new jsPDF('p', 'mm', 'a4');

    /* ==================================
    COVER PAGE
    ================================== */

    pdf.setFillColor(15, 47, 107);

    pdf.rect(
      0,
      0,
      210,
      297,
      'F'
    );

    pdf.setTextColor(
      255,
      255,
      255
    );

    pdf.setFontSize(28);

    pdf.text(
      'Payroll Intelligence Report',
      20,
      90
    );

    pdf.setFontSize(15);

    pdf.text(
      `Generated On: ${new Date().toLocaleDateString()}`,
      20,
      110
    );

    pdf.text(
      `Payroll Health Score: ${this.healthScore}/100`,
      20,
      125
    );

    pdf.text(
      `Risk Level: ${this.riskLevel}`,
      20,
      140
    );

    /* ==================================
    EXECUTIVE SUMMARY
    ================================== */

    pdf.addPage();

    pdf.setTextColor(
      0,
      0,
      0
    );

    pdf.setFontSize(20);

    pdf.text(
      'Executive Summary',
      15,
      20
    );

    const summaryText =
      pdf.splitTextToSize(
        this.executiveReport.executiveSummary,
        180
      );

    pdf.setFontSize(11);

    pdf.text(
      summaryText,
      15,
      35
    );

    /* ==================================
    DASHBOARD OVERVIEW
    ================================== */

    pdf.addPage();

    pdf.setFontSize(20);

    pdf.text(
      'Dashboard Overview',
      15,
      20
    );

    let y = 40;

    pdf.setFontSize(12);

    pdf.text(
      `Payroll Cycles: ${this.summary?.totalCycles ?? 0}`,
      20,
      y
    );

    y += 12;

    pdf.text(
      `Total Employees: ${this.summary?.totalEmployees ?? 0}`,
      20,
      y
    );

    y += 12;

    pdf.text(
      `Employees Paid: ${this.summary?.totalEmployeesPaid ?? 0}`,
      20,
      y
    );

    y += 12;

    pdf.text(
      `Pending Approvals: ${this.summary?.totalPendingApprovals ?? 0}`,
      20,
      y
    );

    y += 12;

    pdf.text(
      `Total Anomalies: ${this.summary?.totalAnomalies ?? 0}`,
      20,
      y
    );

    /* ==================================
    CHARTS
    ================================== */

    pdf.addPage();

    await this.addChartPage(
      pdf,
      'employeeChartSection',
      'Employee Status'
    );

    pdf.addPage();

    await this.addChartPage(
      pdf,
      'approvalChartSection',
      'Approval Distribution'
    );

    pdf.addPage();

    await this.addChartPage(
      pdf,
      'trendChartSection',
      'Payroll Anomaly Trend'
    );

    pdf.addPage();

    await this.addChartPage(
      pdf,
      'riskChartSection',
      'Department Risk Scores'
    );

    /* ==================================
    RISK SCORE TABLE
    ================================== */

    pdf.addPage();

    this.addRiskScoreTable(pdf);

    /* ==================================
    KEY INSIGHTS
    ================================== */

    if (this.executiveReport?.keyInsights?.length) {

      
pdf.addPage();

pdf.setFontSize(20);

pdf.text(
  'Key Insights',
  15,
  20
);

let insightY = 40;

pdf.setFontSize(11);

this.executiveReport.keyInsights
  .forEach((insight: string) => {

    pdf.text(
      `• ${ insight } `,
      20,
      insightY
    );

    insightY += 12;
  });


    }

    /* ==================================
    RECOMMENDATIONS
    ================================== */

    if (this.executiveReport?.recommendations?.length) {

      
pdf.addPage();

pdf.setFontSize(20);

pdf.text(
  'Recommendations',
  15,
  20
);

let recY = 40;

pdf.setFontSize(11);

this.executiveReport.recommendations
  .forEach((rec: string) => {

    pdf.text(
      `• ${ rec } `,
      20,
      recY
    );

    recY += 12;
  });

    }

    /* ==================================
    FOOTER
    ================================== */

    const totalPages =
      pdf.getNumberOfPages();

    for (
      let i = 1;
      i <= totalPages;
      i++
    ) {

     
pdf.setPage(i);

pdf.setFontSize(10);

pdf.text(
  `Page ${ i } of ${ totalPages } `,
  170,
  290
);


    }

    pdf.save(
      `Payroll_Report_${new Date()
        .toISOString()
        .split('T')[0]
      }.pdf`
    );
  }
  private async addChartPage(
    pdf: jsPDF,
    elementId: string,
    title: string
  ): Promise<void> {

    const element =
      document.getElementById(elementId);

    if (!element) {
      console.error(
        `Element ${elementId} not found`
      );
      return;
    }

    pdf.setFontSize(18);

    pdf.text(
      title,
      15,
      20
    );

    const canvas =
      await html2canvas(
        element,
        {
          scale: 2,
          backgroundColor: '#ffffff'
        }
      );

    const image =
      canvas.toDataURL('image/png');

    pdf.addImage(
      image,
      'PNG',
      10,
      30,
      190,
      100
    );
  }
  private addRiskScoreTable(
    pdf: jsPDF
  ): void {

    pdf.setFontSize(18);

    pdf.text(
      'Department Risk Details',
      15,
      20
    );

    let y = 40;

    pdf.setFontSize(11);

    this.riskScores.forEach(risk => {

      pdf.text(
        `${risk.department} | Score: ${risk.riskScore} | Level: ${risk.riskLevel}`,
        15,
        y
      );

      y += 8;

      if (y > 270) {

        pdf.addPage();

        y = 20;
      }
    });
  }

}
