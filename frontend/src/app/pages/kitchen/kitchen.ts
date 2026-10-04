import { DatePipe } from '@angular/common';
import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { GetOrderDTO } from '@dtos/order/get-order-dto';
import { NotificationEventEnum } from '@enums/notification-event-enum';
import { OrderStatusEnum } from '@enums/order-status-enum';
import { KitchenService } from '@services/kitchen-service';
import { SignalrService } from '@services/signalr-service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-kitchen',
  imports: [DatePipe, MatButtonModule, MatTableModule],
  styleUrl: './kitchen.scss',
  templateUrl: './kitchen.html',
})
export class Kitchen implements OnInit {
  private readonly _kitchenService = inject(KitchenService);
  private readonly _signalrService = inject(SignalrService);
  private readonly _destroyRef = inject(DestroyRef);

  public readonly currentColumns: string[] = ['name', 'quantity'];
  public readonly waitingColumns: string[] = ['code', 'customer', 'pizzas', 'createdAt'];

  public readonly queue = signal<GetOrderDTO[]>([]);
  public readonly isBusy = signal(false);

  // The queue holds every order that is not ready: at most one is in progress (the oldest one),
  // the others are pending

  public readonly currentOrder = computed(
    () => this.queue().find((order) => order.status === OrderStatusEnum.InProgress) ?? null,
  );
  public readonly waitingOrders = computed(() =>
    this.queue().filter((order) => order.status === OrderStatusEnum.Pending),
  );

  ngOnInit() {
    this._signalrService
      .listen<void>(NotificationEventEnum.QueueChanged)
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this.loadQueue());
    this._signalrService.reconnected$
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this.loadQueue());
    this._destroyRef.onDestroy(() => this._signalrService.stopConnection());

    // Join the group before the first GET, otherwise an update in between would be missed
    this._signalrService.joinKitchen().then(() => this.loadQueue());
  }

  takeNext() {
    this.runAction(this._kitchenService.takeNextOrder());
  }

  complete(code: string) {
    this.runAction(this._kitchenService.completeOrder(code));
  }

  pizzasOf(order: GetOrderDTO): string {
    return order.items.map((item) => `${item.quantity}× ${item.pizzaName}`).join(', ');
  }

  private runAction(action: Observable<GetOrderDTO>) {
    this.isBusy.set(true);

    /// The interceptor shows the errors. Reload in any case, the queue may have changed
    action.subscribe({
      next: () => this.afterAction(),
      error: () => this.afterAction(),
    });
  }

  // Enable the buttons and update the queue
  private afterAction() {
    this.isBusy.set(false);
    this.loadQueue();
  }

  private loadQueue() {
    this._kitchenService.getQueue().subscribe((queue) => this.queue.set(queue));
  }
}
