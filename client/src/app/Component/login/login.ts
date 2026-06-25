import { Component, inject, signal, } from "@angular/core";
import { AccountService } from "../../../core/services/account-service";
import { FormsModule } from "@angular/forms";
import { Router } from "@angular/router";  
import { CommonModule } from "@angular/common";   
import { Register } from "../register/register";
import { Home } from "../home/home";
import { MatSnackBar } from '@angular/material/snack-bar';


@Component({
  selector: 'app-login',
  imports: [FormsModule, CommonModule,Register,Home],  
  templateUrl:'./login.html',
  styleUrl: './login.css'
})
export class Login {
  public account = inject(AccountService);
  protected LoginData: any = {};
  protected registerpage = signal(false);

  private router = inject(Router);


  
  ngOnInit() {
    console.log('Current user:', this.account.currentUser());

    if (this.account.currentUser()) {
      this.router.navigate(['/home']);
    }
  }
  constructor(
    private snackBar: MatSnackBar) { }


  login() {
    this.account.login(this.LoginData).subscribe({
      next: result => {
        console.log(result);

        this.account.loggedIn.set(true);
        this.LoginData = {};

        this.snackBar.open('Login successful!', 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });

        this.router.navigateByUrl('/home');
      },

      error: err => {
        console.log(err);

        this.snackBar.open(
          err.error?.message || 'Login failed. Please try again.',
          'Close',
          {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
            panelClass: ['error-snackbar']
          }
        );
      }
    });
  }
  showRegisterMode() {
    this.registerpage.set(true);
    this.router.navigateByUrl('/register');
  }
  
}

