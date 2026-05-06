import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CartResponse } from '../../shared/models/models';
import { CartService } from '../../shared/services/cart.service';
import { OrderService } from '../../shared/services/order.service';

@Component({
  selector: 'app-cart',
  standalone: false,
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.scss']
})
export class CartComponent implements OnInit {

  cart: CartResponse = { items: [], grandTotal: 0 };
  loading = false;
  checkingOut = false;
  errorMessage = '';

  constructor(
    private cartService: CartService,
    private orderService: OrderService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    this.loading = true;
    this.cartService.getCart().subscribe({
      next: (data) => {
        this.cart = data;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load cart.';
        this.loading = false;
      }
    });
  }

  updateQuantity(cartItemId: number, quantity: number): void {
    if (quantity < 1) return;

    this.cartService.updateItem(cartItemId, { quantity }).subscribe({
      next: () => this.loadCart(),
      error: () => this.errorMessage = 'Failed to update quantity.'
    });
  }

  removeItem(cartItemId: number): void {
    this.cartService.removeItem(cartItemId).subscribe({
      next: () => this.loadCart(),
      error: () => this.errorMessage = 'Failed to remove item.'
    });
  }

  checkout(): void {
    this.checkingOut = true;
    this.errorMessage = '';

    this.orderService.checkout().subscribe({
      next: (order) => {
        this.checkingOut = false;
        // Navigate to payment page with the new order id
        this.router.navigate(['/payment', order.id]);
      },
      error: (err) => {
        this.checkingOut = false;
        this.errorMessage = err.error || 'Checkout failed. Please try again.';
      }
    });
  }
}
