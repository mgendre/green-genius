import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { provideSpartanHlm } from '@spartan-ng/helm/utils';
import { routes } from './app.routes';
import { API_BASE_URL } from './api/api-client.generated';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    provideSpartanHlm(),
    { provide: API_BASE_URL, useValue: '/api' },
    provideTranslateService({ lang: 'fr', fallbackLang: 'fr' }),
    provideTranslateHttpLoader({ prefix: '/i18n/', suffix: '.json' }),
  ],
};
