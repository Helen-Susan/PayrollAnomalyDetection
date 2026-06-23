import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DashboardSummary } from '../../app/Models/dashboard-summary';
import { DepartmentRiskScoreDto } from '../../app/Models/department-risk-score';
import { ExecutiveReportResponseDto } from '../../app/Models/exectuive-report-response';
@Injectable({
  providedIn: 'root'
})
export class ReportService {

  private apiUrl =
    'https://localhost:7083/api/v1/reports';
  summary: any;
  riskScores: any[] = [];

  executiveReport: any;

  healthScore = 0;

  constructor(
    private http: HttpClient
  ) { }

  getDashboardSummary():
    Observable<DashboardSummary> {

    return this.http.get<DashboardSummary>(
      `${this.apiUrl}/summary`
    );
  }
  getRiskScores() {
    return this.http.get<
      DepartmentRiskScoreDto[]
    >(
      `${this.apiUrl}/risk-score`
    );
  }
  generateExecutiveReport() {

    return this.http.get<ExecutiveReportResponseDto>(
      `${this.apiUrl}/executive`,
      
     
     
    );

  }
  exportPdf(): Observable<Blob> {

    return this.http.get(
      `${this.apiUrl}/export-pdf`,
      {
        responseType: 'blob'
      }
    );
  }
}
