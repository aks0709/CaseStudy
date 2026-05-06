import { Component, OnInit } from '@angular/core';
import { timer } from 'rxjs';
import { MerchantResponse, ProductResponse } from '../../shared/models/models';
import { MerchantService } from '../../shared/services/merchant.service';
import { ProductService } from '../../shared/services/product.service';
import { CartService } from '../../shared/services/cart.service';

@Component({
  selector: 'app-catalog',
  standalone: false,
  templateUrl: './catalog.component.html',
  styleUrls: ['./catalog.component.scss']
})
export class CatalogComponent implements OnInit {

  merchants: MerchantResponse[] = [];
  // Products grouped by merchantId
  productsByMerchant: { [merchantId: number]: ProductResponse[] } = {};
  expandedMerchantId: number | null = null;
  addingProductId: number | null = null;
  successMessage = '';
  errorMessage = '';

  constructor(
    private merchantService: MerchantService,
    private productService: ProductService,
    private cartService: CartService
  ) {}

  ngOnInit(): void {
    this.loadMerchants();
  }

  loadMerchants(): void {
    this.merchantService.getAll().subscribe({
      next: (data) => this.merchants = data,
      error: () => this.errorMessage = 'Failed to load merchants.'
    });
  }

  // Toggle merchant expansion and load their products
  toggleMerchant(merchantId: number): void {
    if (this.expandedMerchantId === merchantId) {
      this.expandedMerchantId = null;
      return;
    }

    this.expandedMerchantId = merchantId;

    if (!this.productsByMerchant[merchantId]) {
      this.productService.getByMerchant(merchantId).subscribe({
        next: (products) => { this.productsByMerchant = { ...this.productsByMerchant, [merchantId]: products }; },
        error: () => this.errorMessage = 'Failed to load products.'
      });
    }
  }

  addToCart(product: ProductResponse): void {
    this.addingProductId = product.id;
    this.successMessage = '';
    this.errorMessage = '';

    this.cartService.addItem({ productId: product.id, quantity: 1 }).subscribe({
      next: () => {
        this.addingProductId = null;
        this.successMessage = `"${product.name}" added to cart!`;
        timer(3000).subscribe(() => this.successMessage = '');
      },
      error: (err) => {
        this.addingProductId = null;
        this.errorMessage = err.error || 'Failed to add to cart.';
      }
    });
  }
}
