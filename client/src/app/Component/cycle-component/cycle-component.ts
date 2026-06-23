// import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
// import { PayrollService } from '../../../core/services/Payroll-service'; 
// import { CycleSummary } from '../../Models/Payroll-cycle.model';

// @Component({
//   selector: 'app-cycle-summary-modal',
//   templateUrl: './cycle-summary-modal.component.html',
//   styleUrls: ['./cycle-summary-modal.component.scss']
// })
// export class CycleSummaryModalComponent implements OnInit {
//   @Input() cycleId!: string;
//   @Output() closed = new EventEmitter<void>();

//   summary: CycleSummary | null = null;
//   isLoading = true;
//   errorMessage = '';

//   constructor(private payrollService: PayrollService) { }

//   ngOnInit(): void {
//     this.loadSummary();
//   }

//   loadSummary(): void {
//     this.isLoading = true;
//     this.errorMessage = '';
//     GET /api/v1/payroll/cycles/{cycleId}
//     this.payrollService.getCycleSummary(this.cycleId).subscribe({
//       next: (data) => {
//         this.summary = data;
//         this.isLoading = false;
//       },
//       error: (err) => {
//         this.errorMessage = err.message;
//         this.isLoading = false;
//       }
//     });
//   }

//   onClose(): void {
//     this.closed.emit();
//   }
// }
