import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { OrderItemResponse, OrderResponse } from '../models/models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class OrderService {

  private readonly baseUrl = `${environment.apiUrl}/api/order`;

  constructor(private http: HttpClient) {}

  // Customer — convert cart to order
  checkout(): Observable<OrderResponse> {
    return this.http.post<OrderResponse>(`${this.baseUrl}/checkout`, {});
  }

  // Customer — all orders
  getMyOrders(): Observable<OrderResponse[]> {
    return this.http.get<OrderResponse[]>(this.baseUrl);
  }

  // Customer — single order
  getById(id: number): Observable<OrderResponse> {
    return this.http.get<OrderResponse>(`${this.baseUrl}/${id}`);
  }

  // Merchant — order items belonging to them
  getMerchantItems(): Observable<OrderItemResponse[]> {
    return this.http.get<OrderItemResponse[]>(`${this.baseUrl}/merchant-items`);
  }
}
