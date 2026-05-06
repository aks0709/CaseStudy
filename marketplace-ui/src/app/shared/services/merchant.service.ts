import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateMerchantRequest, MerchantResponse } from '../models/models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MerchantService {

  private readonly baseUrl = `${environment.apiUrl}/api/merchant`;

  constructor(private http: HttpClient) {}

  // Public — browse all merchants
  getAll(): Observable<MerchantResponse[]> {
    return this.http.get<MerchantResponse[]>(this.baseUrl);
  }

  // Public — get single merchant
  getById(id: number): Observable<MerchantResponse> {
    return this.http.get<MerchantResponse>(`${this.baseUrl}/${id}`);
  }

  // Merchant role — create profile
  createProfile(request: CreateMerchantRequest): Observable<MerchantResponse> {
    return this.http.post<MerchantResponse>(`${this.baseUrl}/profile`, request);
  }

  // Merchant role — get own profile
  getMyProfile(): Observable<MerchantResponse> {
    return this.http.get<MerchantResponse>(`${this.baseUrl}/my-profile`);
  }
}
