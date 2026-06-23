import { Routes } from '@angular/router';
import { Login } from './Component/login/login';
import { Home } from './Component/home/home';
import { Register } from './Component/register/register';
import { UploadFilesComponent } from './Component/upload/upload';
import { DashboardComponent } from './Component/dashboard/dashboard'

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    component: Login
  },
  {
    path: 'home',
    component: Home
  },
  {
    path: 'register',
    component: Register
  },
  {
    path: "upload",
    component: UploadFilesComponent
  },
  {
    path: "dashboard",
    component: DashboardComponent,
    }
];
