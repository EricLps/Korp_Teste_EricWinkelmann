import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { InvoiceService, Invoice } from '../services/invoice.service';

import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';
import { ProductService, Product } from '../services/product.service';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule,
    MatSelectModule,
    DatePipe
  ],
  templateUrl: './invoices.component.html',
  styleUrls: ['./invoices.component.scss']
})
export class InvoicesComponent implements OnInit {
  invoices: Invoice[] = [];
  products: Product[] = [];
  displayedColumns: string[] = ['number', 'createdAt', 'status', 'items'];

  invoiceForm: FormGroup;
  isLoading = false;

  // Lista temporária de itens da nota
  selectedItems: { productId: string; productName: string; quantity: number }[] = [];

  constructor(
    private fb: FormBuilder,
    private invoiceService: InvoiceService,
    private productService: ProductService,
    private snackBar: MatSnackBar
  ) {
    this.invoiceForm = this.fb.group({
      productId: ['', Validators.required],
      quantity: ['', [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    this.loadInvoices();
    this.loadProducts();
  }

  loadProducts() {
    this.productService.list().subscribe(data => {
      this.products = data;
    });
  }

  loadInvoices() {
    this.invoiceService.list().subscribe(data => {
      this.invoices = data;
    });
  }

  addItem() {
    if (this.invoiceForm.invalid) return;

    const formValue = this.invoiceForm.value;
    const selectedProduct = this.products.find(p => p.id === formValue.productId);

    if (selectedProduct) {
      const qtyToAdd = Number(formValue.quantity);

      // Verifica se já tem no carrinho para somar a quantidade
      const existingItemIndex = this.selectedItems.findIndex(i => i.productId === selectedProduct.id);
      const currentCartQty = existingItemIndex >= 0 ? this.selectedItems[existingItemIndex].quantity : 0;

      // Validação de Estoque
      if (currentCartQty + qtyToAdd > (selectedProduct.availableBalance || 0)) {
        this.snackBar.open(`Erro: O saldo disponível é de apenas ${selectedProduct.availableBalance}.`, 'Fechar', { duration: 4000 });
        return;
      }

      if (existingItemIndex >= 0) {
        // Se já existe, só incrementa a quantidade pra não criar uma linha nova
        this.selectedItems[existingItemIndex].quantity += qtyToAdd;
      } else {
        // Cria uma nova linha
        this.selectedItems.push({
          productId: selectedProduct.id!,
          productName: selectedProduct.description,
          quantity: qtyToAdd
        });
      }

      this.invoiceForm.reset();
    }
  }

  getRemainingBalance(product: Product): number {
    const cartItem = this.selectedItems.find(i => i.productId === product.id);
    const cartQty = cartItem ? cartItem.quantity : 0;
    return (product.availableBalance || 0) - cartQty;
  }

  removeItem(index: number) {
    this.selectedItems.splice(index, 1);
  }

  submitInvoice() {
    if (this.selectedItems.length === 0) return;

    this.isLoading = true;

    const payload = {
      items: this.selectedItems.map(item => ({
        productId: item.productId,
        productName: item.productName,
        quantity: item.quantity
      }))
    };

    this.invoiceService.create(payload).pipe(
      catchError(err => {
        let msg = 'Erro ao emitir nota fiscal.';
        if (err.error && err.error.detail) {
          msg = err.error.detail;
        }
        this.snackBar.open(msg, 'Fechar', { duration: 5000 });
        return of(null);
      }),
      finalize(() => this.isLoading = false)
    ).subscribe(result => {
      if (result) {
        this.snackBar.open('Nota Fiscal emitida com sucesso!', 'OK', { duration: 3000 });
        this.selectedItems = [];
        this.loadInvoices();
      }
    });
  }
}
