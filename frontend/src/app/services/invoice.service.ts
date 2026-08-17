import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface InvoiceItem { productId: string; productName?: string; quantity: number }
export interface Invoice { 
  id: string; 
  number?: number; 
  status: string; 
  createdAt?: string; 
  expiresAt?: string; 
  items: InvoiceItem[] 
}

@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private baseUrl = environment.billingApiUrl + '/api/invoices';
  
  constructor(private http: HttpClient) {}

  list(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(this.baseUrl);
  }

  create(payload: { items: InvoiceItem[] }) {
    return this.http.post<Invoice>(this.baseUrl, payload);
  }

  print(invoiceId: string) {
    return this.http.post(`${this.baseUrl}/${invoiceId}/print`, {});
  }
}
