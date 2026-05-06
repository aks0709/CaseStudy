import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({ providedIn: 'root' })
export class CustomerGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) {}

  canActivate(): boolean {
    if (this.authService.isCustomer()) {
      return true;
    }
    // Redirect merchant to their dashboard
    this.router.navigate(['/merchant/dashboard']);
    return false;
  }
}
