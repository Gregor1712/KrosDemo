import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import {provideHttpClient} from "@angular/common/http";

import { routes } from './app.routes';
import {API_BASE_URL} from "./service/api-client";

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(),
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes), {provide: API_BASE_URL, useValue: 'https://localhost:5001'}
  ]
};
