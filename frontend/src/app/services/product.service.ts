import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Product { 
  id?: string; 
  code: string; 
  description: string; 
  balance: number; 
  availableBalance?: number;
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private baseUrl = environment.stockApiUrl + '/api/products';
  
  constructor(private http: HttpClient) {}

  list(): Observable<Product[]> {
    return this.http.get<Product[]>(this.baseUrl);
  }

  create(payload: Product): Observable<Product> {
    return this.http.post<Product>(this.baseUrl, payload);
  }

  reserve(productId: string, payload: { invoiceId: string; quantity: number }) {
    return this.http.post(`${this.baseUrl}/${productId}/reserve`, payload);
  }
}
