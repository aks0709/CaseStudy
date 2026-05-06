import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideZoneChangeDetection } from '@angular/core';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { jwtInterceptor } from './shared/interceptors/jwt.interceptor';
import { NavbarComponent } from './shared/components/navbar.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@NgModule({
  declarations: [
    App,
    NavbarComponent
  ],
  imports: [
    BrowserModule,
    CommonModule,
    RouterModule,
    AppRoutingModule
  ],
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideHttpClient(withInterceptors([jwtInterceptor]))
  ],
  bootstrap: [App]
})
export class AppModule {}
// provideZoneChangeDEtection wires zone.js so that Angular can detect async events and trigger change detection.
//and change Detection is responsible for updating the UI when data changes.
//provideHttpClient sets up the Angular HttpClient with the specified interceptors, in this case,
//  the jwtInterceptor which adds the JWT token to outgoing HTTP requests.