import { Component, inject, signal } from "@angular/core";
import { AccountService } from "../../../core/services/account-service";
import { FormsModule } from "@angular/forms";
import { Router } from "@angular/router";  
import { CommonModule } from "@angular/common";   
import { Register } from "../register/register";
import { Home } from "../home/home";

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

  login() {
    this.account.login(this.LoginData).subscribe({
    next:result=> {
        console.log(result);
        this.account.loggedIn.set(true);
        this.LoginData = {};
        this.router.navigateByUrl('/home');
      },
      error: err => console.log(err.message),
       

    });
  }
  showRegisterMode() {
    this.registerpage.set(true);
    this.router.navigateByUrl('/register');
  }
  
}

