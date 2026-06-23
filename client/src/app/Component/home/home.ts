import { Component ,inject} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AccountService } from "../../../core/services/account-service";
import { SidebarComponent } from '../shared/sidenavcomponent/sidenavcomponent';
import { DashboardComponent } from '../dashboard/dashboard';
import { PayrollService } from "../../../core/services/Payroll-service"
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { ChangeDetectorRef } from '@angular/core';



@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, SidebarComponent, MatMenuModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class Home {
  private router = inject(Router);
  private account = inject(AccountService);
  private payroll = inject(PayrollService);
  private cdr = inject(ChangeDetectorRef);

  showNotifications = false;
  recentAnomalies: any[] = [];
  ngOnInit() {

    console.log(this.account.currentUser());

    this.loadNotifications();

  }
  get username(): string {

    return this.account.currentUser()?.displayName ?? 'Guest';

  }

 
  logout() {
    this.account.logout();
    this.account.loggedIn.set(false);
    this.router.navigateByUrl('/');
  }


navigateToUpload(): void {
  this.router.navigate(['/upload']);
}
 

  loadNotifications(): void {

    this.payroll
      .getNotifications()
      .subscribe({
        next: data => {
          console.log(data);
          console.log("length:",data.length)

          this.recentAnomalies = data;
          console.log("Length:", this.recentAnomalies.length);

          this.cdr.detectChanges();
        },
        error: (err) => { console.error(err); }

      });
  }
  test() {

    console.log(
      "Template Array:",
      this.recentAnomalies);

    console.log(
      "Template Length:",
      this.recentAnomalies.length);
  }
}
