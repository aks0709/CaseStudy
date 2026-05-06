import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { InitiatePaymentResponse, PaymentStatusResponse } from '../../shared/models/models';
import { PaymentService } from '../../shared/services/payment.service';

@Component({
  selector: 'app-payment',
  standalone: false,
  templateUrl: './payment.component.html',
  styleUrls: ['./payment.component.scss']
})
export class PaymentComponent implements OnInit {

  orderId!: number;
  payment: InitiatePaymentResponse | null = null;
  status: PaymentStatusResponse | null = null;
  loading = false;
  completing = false;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private paymentService: PaymentService
  ) {}

  ngOnInit(): void {
    this.orderId = Number(this.route.snapshot.paramMap.get('orderId'));
    this.initiatePayment();
  }

  initiatePayment(): void {
    this.loading = true;

    this.paymentService.initiate(this.orderId).subscribe({
      next: (data) => {
        this.payment = data;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error || 'Failed to initiate payment.';
      }
    });
  }

  // Simulate clicking "Pay Now" on the dummy gateway
  completePayment(): void {
    if (!this.payment) return;

    this.completing = true;
    this.errorMessage = '';

    this.paymentService.complete(this.payment.paymentId).subscribe({
      next: (data) => {
        this.status = data;
        this.completing = false;
      },
      error: (err) => {
        this.completing = false;
        this.errorMessage = err.error || 'Payment completion failed.';
      }
    });
  }

  goToOrders(): void {
    this.router.navigate(['/order/history']);
  }
}
