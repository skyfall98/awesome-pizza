import { CurrencyPipe } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute } from '@angular/router';
import { CcSnackbarService } from '@common-components/cc-snackbar/cc-snackbar-service';
import { GetOrderDTO } from '@dtos/order/get-order-dto';
import { OrderStatusChangedDTO } from '@dtos/notification/order-status-changed-dto';
import { NotificationEventEnum } from '@enums/notification-event-enum';
import { NotificationLevelEnum } from '@enums/notification-level-enum';
import { OrderStatusEnum } from '@enums/order-status-enum';
import { OrderService } from '@services/order-service';
import { SignalrService } from '@services/signalr-service';

@Component({
  selector: 'app-tracking',
  imports: [CurrencyPipe, MatProgressBarModule, MatTableModule],
  styleUrl: './tracking.scss',
  templateUrl: './tracking.html',
})
export class Tracking implements OnInit {
  private readonly _route = inject(ActivatedRoute);
  private readonly _orderService = inject(OrderService);
  private readonly _signalrService = inject(SignalrService);
  private readonly _snackbarService: CcSnackbarService = inject(CcSnackbarService);
  private readonly _destroyRef = inject(DestroyRef);

  private readonly _code = this._route.snapshot.paramMap.get('code')!;

  public readonly OrderStatusEnum = OrderStatusEnum;
  public readonly displayedColumns: string[] = ['name', 'quantity', 'price'];
  public readonly statusLabels: Record<OrderStatusEnum, string> = {
    [OrderStatusEnum.Pending]: 'In attesa',
    [OrderStatusEnum.InProgress]: 'In preparazione',
    [OrderStatusEnum.Ready]: 'Pronto',
  };

  public readonly order = signal<GetOrderDTO | null>(null);
  public readonly loadFailed = signal(false);

  ngOnInit() {
    // No filter on the code: the server sends this event only to the group of this order
    this._signalrService
      .listen<OrderStatusChangedDTO>(NotificationEventEnum.OrderStatusChanged)
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((notification) => {
        if (notification.status === OrderStatusEnum.Ready) {
          this._snackbarService.openSnack(NotificationLevelEnum.Success, 'Il tuo ordine è pronto!');
        }
        this.loadOrder();
      });
    this._signalrService.reconnected$
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this.loadOrder());
    this._destroyRef.onDestroy(() => this._signalrService.stopConnection());

    // Subscribe to the order before the first GET, otherwise an update in between would be missed
    this._signalrService.subscribeToOrder(this._code).then(() => this.loadOrder());
  }

  private loadOrder() {
    this._orderService.getOrderByCode(this._code).subscribe({
      next: (order) => this.order.set(order),
      error: () => this.loadFailed.set(true),
    });
  }
}
