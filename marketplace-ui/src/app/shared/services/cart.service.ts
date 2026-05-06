import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AddCartItemRequest, CartItemResponse, CartResponse, UpdateCartItemRequest } from '../models/models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CartService {

  private readonly baseUrl = `${environment.apiUrl}/api/cart`;

  constructor(private http: HttpClient) {}

  getCart(): Observable<CartResponse> {
    return this.http.get<CartResponse>(this.baseUrl);
  }

  addItem(request: AddCartItemRequest): Observable<CartItemResponse> {
    return this.http.post<CartItemResponse>(this.baseUrl, request);
  }

  updateItem(cartItemId: number, request: UpdateCartItemRequest): Observable<CartItemResponse> {
    return this.http.put<CartItemResponse>(`${this.baseUrl}/${cartItemId}`, request);
  }

  removeItem(cartItemId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${cartItemId}`);
  }
}
