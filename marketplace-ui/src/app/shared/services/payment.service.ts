import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { InitiatePaymentResponse, PaymentStatusResponse } from '../models/models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PaymentService {

  private readonly baseUrl = `${environment.apiUrl}/api/payment`;

  constructor(private http: HttpClient) {}

  // Initiate payment for an order — returns dummy gateway URL
  initiate(orderId: number): Observable<InitiatePaymentResponse> {
    return this.http.post<InitiatePaymentResponse>(`${this.baseUrl}/initiate/${orderId}`, {});
  }

  // Simulate gateway callback — marks payment and order as Paid
  complete(paymentId: number): Observable<PaymentStatusResponse> {
    return this.http.post<PaymentStatusResponse>(`${this.baseUrl}/complete/${paymentId}`, {});
  }

  // Check payment and order status
  getStatus(orderId: number): Observable<PaymentStatusResponse> {
    return this.http.get<PaymentStatusResponse>(`${this.baseUrl}/status/${orderId}`);
  }
}
