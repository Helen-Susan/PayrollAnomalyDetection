import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { HttpEventType,HttpEvent } from '@angular/common/http';
import { Subject, interval } from 'rxjs';
import { takeUntil, switchMap } from 'rxjs/operators';
import { PayrollService } from '../../../core/services/Payroll-service';
import { AccountService } from '../../../core/services/account-service'
import { PayrollCycle, CycleSummary } from '../../Models/Payroll-cycle.model';
import { CycleStatus } from '../../types/upload'; 
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FilterOption } from '../../types/upload';
import { SidebarComponent } from '../shared/sidenavcomponent/sidenavcomponent';



@Component({
  selector: 'app-upload-files',
  templateUrl: 'upload.html',
  styleUrls: ['upload.css'],
  imports: [CommonModule, FormsModule, SidebarComponent],
  standalone: true
})
export class UploadFilesComponent implements  OnDestroy,OnInit {

  // ── Upload state ─────────────────────────────────────
  isDragging = false;
  

  // ── Cycles state ─────────────────────────────────────
  cycles: PayrollCycle[] = [];
  isLoadingCycles = false;
  searchQuery = '';
   account = inject(AccountService);


  // ── Filter ───────────────────────────────────────────
  filters: FilterOption[] = ['All', 'Pending', 'Done', 'Error','Review Required','Processing'];
  activeFilter: FilterOption = 'All';

  // ── Modal ────────────────────────────────────────────
  selectedCycleId: string | null = null;
  selectedSummary: CycleSummary | null = null;

  showSummaryModal: boolean = false;

  // ── Status legend ────────────────────────────────────
  readonly statusGuide = [
    { key: 'pending', label: 'Pending — queued for processing' },
    { key: 'processing', label: 'Processing — engine running' },
    { key: 'done', label: 'Done — cycle complete' },
    { key: 'error', label: 'Error — action required' },
  ];

  private destroy$ = new Subject<void>();

  // ── Dependency Injection ─────────────────────────────
  constructor(
    private payrollService: PayrollService,
    private snackBar: MatSnackBar
  ) { }

  ngOnInit(): void {
    this.loadCycles();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Computed ─────────────────────────────────────────
  get filteredCycles(): PayrollCycle[] {
    const q = this.searchQuery.toLowerCase().trim();
    return this.cycles
      .filter(c => this.activeFilter === 'All' || c.status === this.activeFilter)
      .filter(c =>
        !q ||
        c.fileName.toLowerCase().includes(q) ||
        c.id.toLowerCase().includes(q)
      );
  }
  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
  }
  // ── Filters ──────────────────────────────────────────
  setFilter(filter: FilterOption): void {
    this.activeFilter = filter;
  }
  get username(): string {

    return this.account.currentUser()?.displayName ?? 'Guest';

  }


  // ── GET /api/v1/payroll/cycles ───────────────────────
  loadCycles(): void {
    this.isLoadingCycles = true;
    this.payrollService.getAllCycles()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.cycles = data;
          this.isLoadingCycles = false;
        },
        error: () => {
          this.isLoadingCycles = false;
        }
      });
  }

   // GET /api/v1/payroll/cycles/{cycleId}/status ──────
  refreshStatus(cycle: PayrollCycle): void {
    this.payrollService.getCycleStatus(cycle.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          const target = this.cycles.find(c => c.id === cycle.id);
          if (target) target.status = res.status;
        }
      });
  }

  // ── POST /api/v1/payroll/upload ──────────────────────
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) this.uploadFile(file);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = true;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
    const file = event.dataTransfer?.files[0];
    if (file) this.uploadFile(file);
  }

  private uploadFile(file: File): void {

    // this.uploadState = 'uploading';
    // this.uploadProgress = 0;
    // this.uploadError = '';

    this.payrollService
      .uploadPayrollFile(file)
      .pipe(takeUntil(this.destroy$))
      .subscribe({

        next: (event) => {
          console.log("EVENT:", event);

          // Upload Progress Event
          if (event.type === HttpEventType.UploadProgress) {

            if (event.total) {

              // this.uploadProgress = Math.round(
              //   (100 * event.loaded) / event.total
              // );

            }

          }

          // Final Response Event
          if (event.type === HttpEventType.Response) {

            // this.uploadState = 'success';

            // this.uploadProgress = 100;

            this.snackBar.open(
              'Payroll file uploaded successfully!',
              'Close',
              {
                duration: 4000,
                horizontalPosition: 'right',
                verticalPosition: 'top',
                panelClass: ['success-snackbar']
              }
            );

          }
          

        },

        error: (err) => {

          // this.uploadState = 'error';

          // this.uploadError =
          //   err.message ||
          //   'Upload failed. Please try again.';

          this.snackBar.open(
            'File upload failed!',
            'Close',
            {
              duration: 5000,
              horizontalPosition: 'right',
              verticalPosition: 'top',
              panelClass: ['error-snackbar']
            }
          );

        }

      });

  }

  // ── DELETE /api/v1/payroll/cycles/{cycleId} ──────────
  deleteCycle(cycle: PayrollCycle): void {

    if (
      cycle.status === 'Done' ||
      cycle.status === 'Review Required' ||
      cycle.status === 'Processing'
    ) {
      this.snackBar.open(
        `Delete for cycle ${cycle.id} is not permitted while status is ${cycle.status}.`,
        'Close',
        {
          duration: 4000,
          panelClass: ['error-snackbar']
        }
      );

      return;
    }

    if (!confirm(`Delete cycle ${cycle.id}? This cannot be undone.`))
      return;

    this.payrollService.deleteCycle(cycle.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {

          this.cycles =
            this.cycles.filter(c => c.id !== cycle.id);

          this.snackBar.open(
            `Cycle ${cycle.id} deleted successfully.`,
            'Close',
            {
              duration: 3000,
              panelClass: ['success-snackbar']
            }
          );
        },
        error: (err) => {

          this.snackBar.open(
            `Delete failed: ${err.message}`,
            'Close',
            {
              duration: 5000,
              panelClass: ['error-snackbar']
            }
          );
        }
      });
  }

  // ── Modal: GET /api/v1/payroll/cycles/{cycleId} ──────
  openSummary(cycleId:string): void {
    this.payrollService
      .getCycleSummary(cycleId)
      .subscribe({

        next: (summary) => {
          this.selectedSummary =
            summary;

          this.showSummaryModal = true;
          console.log("SUMMARY RESPONSE:", summary);
        },

        error: (err) => {
          console.error(err);
        }

      });
  }

  closeSummaryModal(): void {
    this.showSummaryModal = false;

    this.selectedSummary = null;
  }
}
