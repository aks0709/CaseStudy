import { Component, OnInit } from '@angular/core';
import { timer } from 'rxjs';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MerchantResponse, ProductResponse } from '../../shared/models/models';
import { MerchantService } from '../../shared/services/merchant.service';
import { ProductService } from '../../shared/services/product.service';
import { Validation } from '../../shared/validation';

@Component({
  selector: 'app-merchant-dashboard',
  standalone: false,
  templateUrl: './merchant-dashboard.component.html',
  styleUrls: ['./merchant-dashboard.component.scss']
})
export class MerchantDashboardComponent implements OnInit {

  profile: MerchantResponse | null = null;
  products: ProductResponse[] = [];
  profileForm: FormGroup;
  productForm: FormGroup;
  editingProductId: number | null = null;
  loading = false;
  savingProfile = false;
  savingProduct = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private merchantService: MerchantService,
    private productService: ProductService
  ) {
    this.profileForm = this.fb.group({
      businessName: ['', [Validators.required, Validation.required]],
      description: ['', [Validators.required, Validation.required]]
    });

    this.productForm = this.fb.group({
      name: ['', [Validators.required, Validation.required]],
      description: ['', [Validators.required, Validation.required]],
      price: [null, [Validators.required, Validation.positivePrice]]
    });
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.loading = true;
    this.errorMessage = '';
    this.merchantService.getMyProfile().subscribe({
      next: (data) => {
        this.profile = data;
        this.loading = false;
        this.loadProducts();
      },
      error: (err) => {
        this.loading = false;
        if (err.status === 0) {
          this.errorMessage = 'Cannot reach server. Make sure the backend is running.';
        } else if (err.status !== 404) {
          this.errorMessage = 'Failed to load profile. (Status: ' + err.status + ')';
        }
        // 404 = no profile yet, show setup form
      }
    });
  }

  loadProducts(): void {
    if (!this.profile) return;
    this.productService.getByMerchant(this.profile.id).subscribe({
      next: (data) => this.products = data,
      error: () => this.errorMessage = 'Failed to load products.'
    });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }
    this.savingProfile = true;
    this.errorMessage = '';
    this.merchantService.createProfile(this.profileForm.value).subscribe({
      next: (data) => {
        this.profile = data;
        this.savingProfile = false;
        this.successMessage = 'Profile created! You can now add products.';
        timer(4000).subscribe(() => this.successMessage = '');
        this.loadProducts();
      },
      error: (err) => {
        this.savingProfile = false;
        this.errorMessage = err.error || 'Failed to save profile.';
      }
    });
  }

  saveProduct(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }
    this.savingProduct = true;
    this.errorMessage = '';
    const action = this.editingProductId
      ? this.productService.update(this.editingProductId, this.productForm.value)
      : this.productService.create(this.productForm.value);
    action.subscribe({
      next: () => {
        this.savingProduct = false;
        this.productForm.reset();
        this.editingProductId = null;
        this.successMessage = 'Product saved!';
        timer(3000).subscribe(() => this.successMessage = '');
        this.loadProducts();
      },
      error: (err) => {
        this.savingProduct = false;
        this.errorMessage = err.error || 'Failed to save product.';
      }
    });
  }

  editProduct(product: ProductResponse): void {
    this.editingProductId = product.id;
    this.productForm.patchValue({
      name: product.name,
      description: product.description,
      price: product.price
    });
  }

  cancelEdit(): void {
    this.editingProductId = null;
    this.productForm.reset();
  }

  getError(form: FormGroup, field: string): string {
    return Validation.getError(form.get(field));
  }
}
