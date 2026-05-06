import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../shared/services/auth.service';
import { Validation } from '../../shared/validation';

@Component({
  selector: 'app-auth',
  standalone: false,
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.scss']
})
export class AuthComponent {

  // Toggle between login and register
  isLoginMode = true;
  errorMessage = '';
  loading = false;

  loginForm: FormGroup;
  registerForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validation.email]],
      password: ['', [Validators.required, Validation.password]]
    });

    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required, Validation.required]],
      email: ['', [Validators.required, Validation.email]],
      password: ['', [Validators.required, Validation.password]],
      role: ['Customer', Validators.required]
    });
  }

  toggleMode(): void {
    this.isLoginMode = !this.isLoginMode;
    this.errorMessage = '';
  }

  getError(form: FormGroup, field: string): string {
    return Validation.getError(form.get(field));
  }

  onLogin(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }
    // markAllAsTouched() forces all fields into touched state,
    //  which triggers the template to display validation error messages like:
    // <span class="field-error">{{ getError(loginForm, 'email') }}</span>
    // Without this, if the user clicks "Login" without filling anything,
    //  no error messages would show because the fields are still in pristine/untouched state.

    this.loading = true;  //loaded successfully, but waiting for response from server
    this.errorMessage = '';

    this.authService.login(this.loginForm.value).subscribe({
      next: (response) => {
        this.loading = false;
        this.redirectAfterLogin(response.role);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error || 'Login failed. Please check your credentials.';
      }
    });
  }

  onRegister(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.authService.register(this.registerForm.value).subscribe({
      next: (response) => {
        this.loading = false;
        this.redirectAfterLogin(response.role);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error || 'Registration failed. Please try again.';
      }
    });
  }

  // Route to correct home based on role
  private redirectAfterLogin(role: string): void {
    if (role === 'Merchant') {
      this.router.navigate(['/merchant/products']);
    } else {
      this.router.navigate(['/catalog']);
    }
  }
}
