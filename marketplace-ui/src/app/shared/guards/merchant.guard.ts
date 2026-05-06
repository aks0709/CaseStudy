import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({ providedIn: 'root' })
export class MerchantGuard implements CanActivate {

  constructor(private authService: AuthService, private router: Router) {}

  canActivate(): boolean {
    if (this.authService.isMerchant()) {
      return true;
    }
    // Redirect customer to catalog
    this.router.navigate(['/catalog']);
    return false;
  }
}
