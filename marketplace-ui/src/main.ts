import { platformBrowser } from '@angular/platform-browser';
import { AppModule } from './app/app-module';

platformBrowser().bootstrapModule(AppModule)
  .catch((err: unknown) => console.error(err));
//platformBrowser().bootstrapModule(AppModule) is the standard way
//  to start an Angular application in the browser. 
// It bootstraps the root module (AppModule) which then initializes the app. 
// The catch block is added to log any errors that occur during the bootstrapping process.


//invokes AppModule.ts which is the root module of the Angular application.