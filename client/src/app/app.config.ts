import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { API_BASE_URL } from './service/api-client';
import { etagInterceptor } from './service/etag.interceptor';
import { authInterceptor } from './service/auth.interceptor';
import { unauthorizedInterceptor } from './service/unauthorized.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withInterceptors([authInterceptor, etagInterceptor, unauthorizedInterceptor])
    ),
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    { provide: API_BASE_URL, useValue: '' }
  ]
};
