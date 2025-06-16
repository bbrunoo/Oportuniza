import { ApplicationConfig, importProvidersFrom, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './interceptor/auth.interceptor';
import { provideToastr } from 'ngx-toastr';
import { TranslateModule } from '@ngx-translate/core';
import { provideNgxMask } from 'ngx-mask';
import { MAT_DIALOG_DEFAULT_OPTIONS } from '@angular/material/dialog';


export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes), provideClientHydration(withEventReplay()),
    provideHttpClient(withFetch()),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideToastr(),
    [provideZoneChangeDetection({ eventCoalescing: true }), importProvidersFrom(TranslateModule.forRoot()),],
    provideNgxMask(),

  ]
};


