import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { Router } from '@angular/router';
import { CreateOrderDTO } from '@dtos/order/create-order-dto';
import { GetPizzaDTO } from '@dtos/pizza/get-pizza-dto';
import { OrderService } from '@services/order-service';
import { PizzaService } from '@services/pizza-service';

@Component({
  selector: 'app-order',
  imports: [
    CurrencyPipe,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatTableModule,
    ReactiveFormsModule,
  ],
  templateUrl: './order.html',
  styleUrl: './order.scss',
})
export class Order implements OnInit {
  private readonly _pizzaService = inject(PizzaService);
  private readonly _orderService = inject(OrderService);
  private readonly _router = inject(Router);

  public readonly maxQuantity = 100;
  public readonly displayedColumns: string[] = ['name', 'price', 'quantity'];

  public readonly pizzas = signal<GetPizzaDTO[]>([]);
  public readonly quantities = signal<Record<number, number>>({});
  public readonly isSubmitting = signal(false);

  public readonly customerName = new FormControl('', {
    nonNullable: true,
    validators: Validators.maxLength(100),
  });

  public readonly trackingCode = new FormControl('', { nonNullable: true });

  public readonly totalPrice = computed(() =>
    this.pizzas().reduce((sum, pizza) => sum + pizza.price * this.quantityOf(pizza.pizzaId), 0),
  );
  public readonly hasItems = computed(() =>
    this.pizzas().some((pizza) => this.quantityOf(pizza.pizzaId) > 0),
  );

  ngOnInit() {
    this._pizzaService.getAllPizzas().subscribe((pizzas) => {
      this.pizzas.set(pizzas);
    });
  }

  quantityOf(pizzaId: number): number {
    return this.quantities()[pizzaId] ?? 0;
  }

  increase(pizzaId: number) {
    this.quantities.update((quantities) => ({
      ...quantities,
      [pizzaId]: this.quantityOf(pizzaId) + 1,
    }));
  }

  decrease(pizzaId: number) {
    this.quantities.update((quantities) => ({
      ...quantities,
      [pizzaId]: this.quantityOf(pizzaId) - 1,
    }));
  }

  submit() {
    const dto: CreateOrderDTO = {
      customerName: this.customerName.value.trim() || null,
      items: this.pizzas()
        .filter((pizza) => this.quantityOf(pizza.pizzaId) > 0)
        .map((pizza) => ({ pizzaId: pizza.pizzaId, quantity: this.quantityOf(pizza.pizzaId) })),
    };

    this.isSubmitting.set(true);
    this._orderService.createOrder(dto).subscribe({
      next: (order) => this._router.navigate(['orders', order.code]),
      error: () => this.isSubmitting.set(false),
    });
  }

  track() {
    const code = this.trackingCode.value.trim();
    if (code) {
      this._router.navigate(['orders', code]);
    }
  }
}
