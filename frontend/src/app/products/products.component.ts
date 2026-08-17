import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductService, Product } from '../services/product.service';

import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule
  ],
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.scss']
})
export class ProductsComponent implements OnInit {
  products: Product[] = [];
  displayedColumns: string[] = ['code', 'description', 'balance', 'availableBalance'];

  productForm: FormGroup;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private snackBar: MatSnackBar
  ) {
    this.productForm = this.fb.group({
      code: ['', Validators.required],
      description: ['', Validators.required],
      balance: ['', [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts() {
    this.productService.list().subscribe(data => {
      this.products = data;
    });
  }

  createProduct() {
    if (this.productForm.invalid) return;

    this.isLoading = true;
    const formValue = this.productForm.value;

    this.productService.create(formValue).pipe(
      catchError(err => {
        let msg = 'Erro ao cadastrar produto.';
        if (err.error && err.error.detail) {
          msg = err.error.detail;
        }
        this.snackBar.open(msg, 'Fechar', { duration: 5000 });
        return of(null);
      }),
      finalize(() => this.isLoading = false)
    ).subscribe(result => {
      if (result) {
        this.snackBar.open('Produto cadastrado com sucesso!', 'OK', { duration: 3000 });
        this.productForm.reset();
        this.loadProducts();
      }
    });
  }
}
