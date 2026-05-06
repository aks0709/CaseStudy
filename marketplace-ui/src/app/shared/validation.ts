import { AbstractControl, ValidationErrors } from '@angular/forms';

export class Validation {

  // Email must match standard format
  static email(control: AbstractControl): ValidationErrors | null {
    const value = control.value as string;
    if (!value) return null;
    const valid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
    return valid ? null : { invalidEmail: 'Enter a valid email address.' };
  }

  // Password must be at least 6 characters
  static password(control: AbstractControl): ValidationErrors | null {
    const value = control.value as string;
    if (!value) return null;
    return value.length >= 6 ? null : { weakPassword: 'Password must be at least 6 characters.' };
  }

  // Price must be a positive number
  static positivePrice(control: AbstractControl): ValidationErrors | null {
    const value = control.value as number;
    if (value === null || value === undefined) return null;
    return value > 0 ? null : { invalidPrice: 'Price must be greater than 0.' };
  }

  // Quantity must be at least 1
  static positiveQuantity(control: AbstractControl): ValidationErrors | null {
    const value = control.value as number;
    if (value === null || value === undefined) return null;
    return value >= 1 ? null : { invalidQuantity: 'Quantity must be at least 1.' };
  }

  // Required field with trimming
  static required(control: AbstractControl): ValidationErrors | null {
    const value = control.value as string;
    if (!value || value.toString().trim() === '') {
      return { required: 'This field is required.' };
    }
    return null;
  }

  // Helper: extract first error message from a form control
  static getError(control: AbstractControl | null): string {
    if (!control || !control.errors || !control.touched) return '';
    const errors = control.errors;
    if (errors['required']) return errors['required'];
    if (errors['invalidEmail']) return errors['invalidEmail'];
    if (errors['weakPassword']) return errors['weakPassword'];
    if (errors['invalidPrice']) return errors['invalidPrice'];
    if (errors['invalidQuantity']) return errors['invalidQuantity'];
    return 'Invalid value.';
  }
}
