import { Component, OnInit } from '@angular/core';
import { OrderItemResponse } from '../../shared/models/models';
import { OrderService } from '../../shared/services/order.service';

@Component({
  selector: 'app-merchant-orders',
  standalone: false,
  templateUrl: './merchant-orders.component.html',
  styleUrls: ['./merchant-orders.component.scss']
})
export class MerchantOrdersComponent implements OnInit {

  items: OrderItemResponse[] = [];
  loading = false;
  errorMessage = '';

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.loadItems();
  }

  loadItems(): void {
    this.loading = true;
    this.orderService.getMerchantItems().subscribe({
      next: (data) => {
        this.items = data;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load order items.';
        this.loading = false;
      }
    });
  }

  // Total revenue from all order items
  getTotalRevenue(): number {
    return this.items.reduce((sum, item) => sum + item.lineTotal, 0);
  }
}
