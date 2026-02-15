import { ApplicationConfig, provideZoneChangeDetection, isDevMode } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { appReducer } from './core/store/app.reducer';
import { AppEffects } from './core/store/app.effects';
import { serversReducer } from './core/store/servers/servers.reducer';
import { ServersEffects } from './core/store/servers/servers.effects';
import { metricsReducer } from './core/store/metrics/metrics.reducer';
import { MetricsEffects } from './core/store/metrics/metrics.effects';
import { alertsReducer } from './core/store/alerts/alerts.reducer';
import { AlertsEffects } from './core/store/alerts/alerts.effects';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimationsAsync(),
    provideStore({
      app: appReducer,
      servers: serversReducer,
      metrics: metricsReducer,
      alerts: alertsReducer
    }),
    provideEffects([AppEffects, ServersEffects, MetricsEffects, AlertsEffects]),
    provideStoreDevtools({
      maxAge: 25,
      logOnly: !isDevMode(),
      autoPause: true,
      trace: false,
      traceLimit: 75,
    })
  ]
};
