import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { routes } from './app.routes';

import {
  provideCharts,
  withDefaultRegisterables
} from 'ng2-charts';


  
export const appConfig: ApplicationConfig = {
  providers: [
   provideRouter(routes),
    provideHttpClient(),
    importProvidersFrom(FormsModule),
    provideCharts(
      withDefaultRegisterables()
    )
  ]
};
