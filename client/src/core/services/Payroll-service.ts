import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders,HttpEvent } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { PayrollCycle,  CycleSummary,CycleStatusResponse } from '../../app/Models/Payroll-cycle.model';

@Injectable({
  providedIn: 'root'
})
export class PayrollService {
  private readonly BASE_URL = 'https://localhost:7083/api/v1/payroll';

  constructor(private http: HttpClient) { }

  // POST /api/v1/payroll/upload
  uploadPayrollFile(file: File): Observable<HttpEvent<PayrollCycle>> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http
      .post<PayrollCycle>(`${this.BASE_URL}/upload`, formData, { observe: 'events' ,reportProgress: true})
      .pipe(catchError(this.handleError));
  }

  // GET /api/v1/payroll/cycles
  getAllCycles(): Observable<PayrollCycle[]> {
    return this.http
      .get<PayrollCycle[]>(`${this.BASE_URL}/cycles`)
      .pipe(
        map(cycles =>
          cycles.map(c => ({ ...c, uploadedAt: new Date(c.uploadedAt) }))
        ),
        catchError(this.handleError)
      );
  }

  // GET /api/v1/payroll/cycles/{cycleId}  → returns summary
  getCycleSummary(cycleId: string): Observable<CycleSummary> {
    return this.http
      .get<CycleSummary>(`${this.BASE_URL}/cycles/${cycleId}`)
      .pipe(catchError(this.handleError));
  }

  // GET /api/v1/payroll/cycles/{cycleId}/status
  getCycleStatus(cycleId: string): Observable<CycleStatusResponse> {
    return this.http
      .get<CycleStatusResponse>(`${this.BASE_URL}/cycles/${cycleId}/status`)
      .pipe(catchError(this.handleError));
  }

  // DELETE /api/v1/payroll/cycles/{cycleId}
  deleteCycle(cycleId: string): Observable<void> {
    return this.http
      .delete<void>(`${this.BASE_URL}/cycles/${cycleId}`)
      .pipe(catchError(this.handleError));
  }
  getNotifications(): Observable<any[]> {
    return this.http.get<any[]>(
      `${this.BASE_URL}/notifications`
    );
  }

  private handleError(error: any): Observable<never> {
    const message =
      error?.error?.message || error?.statusText || 'An unknown error occurred';
    return throwError(() => new Error(message));
  }
}
