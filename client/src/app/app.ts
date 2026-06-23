import { Component, inject, OnInit} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AccountService } from '../core/services/account-service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App implements OnInit {
  private accountService = inject(AccountService);
  private http = inject(HttpClient);
  async ngOnInit() {
    this.setCurrentUser();
  }
  setCurrentUser() {
    const userString = localStorage.getItem('user');
    if (!userString) return;

    else {
      this.accountService.currentUser.set(JSON.parse(userString));
    }
  }
}
