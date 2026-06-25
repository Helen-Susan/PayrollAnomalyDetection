import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal, } from '@angular/core';
import { RegisterData, User } from '../../app/types/user';
import { LoginResponse } from '../../app/Models/loginrespose';
import { tap } from 'rxjs';
@Injectable({
  providedIn: 'root',
})
//dependency injection for HttpClient
export class AccountService {
  private http = inject(HttpClient);
  public currentUser = signal<User | null>(null);
  public loggedIn = signal(false);
  baseUrl = 'https://localhost:7083/';
  constructor() {
    const userString = localStorage.getItem('user');


    if (userString) {
      const user: User = JSON.parse(userString);
      this.currentUser.set(user);
    }
  }

  login(loginData: any) {
    return this.http.post<LoginResponse>(
      `${this.baseUrl}auth/login`,
      loginData
    ).pipe(
      tap(response => {

        localStorage.setItem(
          'user',
          JSON.stringify(response.user)
        );

        this.currentUser.set(response.user);

      })
    );
  }
  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
    this.loggedIn.set(false);

  }
  register(registerData: RegisterData) {
    return this.http.post<User>(`${this.baseUrl}register`, registerData);
  }
 
}

