import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../../../../core/services/account-service';


@Component({
  selector: 'app-sidebar',
  standalone: true,
  templateUrl: './sidenavcomponent.html',
  styleUrl: './sidenavcomponent.css'
})
export class SidebarComponent {

  private router = inject(Router);

  private account = inject(AccountService);

  navigateTohome(): void {

    this.router.navigateByUrl('/home');

  }

  navigateToUpload(): void {

    this.router.navigateByUrl('/upload');

  }
  navigateToDashboardSummary(): void {
    this.router.navigateByUrl('/dashboard');
  }
  logout(): void {

    this.account.logout();

    this.router.navigateByUrl('/login');

  }

}
