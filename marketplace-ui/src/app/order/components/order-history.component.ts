import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OrderResponse } from '../../shared/models/models';
import { OrderService } from '../../shared/services/order.service';

@Component({
  selector: 'app-order-history',
  standalone: false,
  templateUrl: './order-history.component.html',
  styleUrls: ['./order-history.component.scss']
})
export class OrderHistoryComponent implements OnInit {

  orders: OrderResponse[] = [];
  loading = false;
  errorMessage = '';

  constructor(private orderService: OrderService, private router: Router) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.orderService.getMyOrders().subscribe({
      next: (data) => {
        this.orders = data;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load orders.';
        this.loading = false;
      }
    });
  }

  // Navigate to payment page if order is still pending
  goToPayment(orderId: number): void {
    this.router.navigate(['/payment', orderId]);
  }
}
