import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./products/products.component').then(m => m.ProductsComponent) },
  { path: 'invoices', loadComponent: () => import('./invoices/invoices.component').then(m => m.InvoicesComponent) }
];
