import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Product { id: string; code: string; description: string; balance: number; availableBalance: number }

@Injectable({ providedIn: 'root' })
export class ProductService {
  private base = '/api/products';
  constructor(private http: HttpClient) {}

  list(): Observable<Product[]> {
    return this.http.get<Product[]>(this.base);
  }

  create(payload: { code: string; description: string; balance: number }) {
    return this.http.post(this.base, payload);
  }

  reserve(productId: string, payload: { invoiceId: string; quantity: number }) {
    return this.http.post(`${this.base}/${productId}/reserve`, payload);
  }
}
