import { Component, inject,signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../../../core/services/account-service';
import { Router } from '@angular/router';
import { Login } from '../login/login';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class Register {
  public account = inject(AccountService);
  private router = inject(Router);
  protected registerpage = signal(false);

   protected registerData = {

    name: '',

    email: '',

    password: ''

  };

  emailError = '';

  passwordError = '';

  successMessage = '';

  register() {

    this.emailError = '';

    this.passwordError = '';

    this.successMessage = '';

    /* EMAIL VALIDATION */

    const emailPattern =
      /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[A-Za-z]{2,}$/;

    if (!emailPattern.test(this.registerData.email)) {

      this.emailError =
        'Please enter a valid email address';

      return;

    }

    /* PASSWORD VALIDATION */

    const passwordPattern =
      /^(?=.*[A-Z])(?=.*\d)(?=.*[@#$%^&*!]).{8,}$/;

    if (!passwordPattern.test(this.registerData.password)) {

      this.passwordError =
        'Password must contain:\n1 Capital Letter\n1 Number\n1 Special Character (@,#,$,%)\nMinimum 8 characters';

      return;

    }

    /* SUCCESS */

  

    this.account.register(this.registerData).subscribe({
      next: result => {
      console.log(result);
     
      //this.registerData = { };
        this.router.navigateByUrl('/login');
        this.successMessage =
          'Registration Successful';
       
    },
      error: err => console.log(err.message),
  });

      }

 

}
