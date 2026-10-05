import { inject, Injectable } from '@angular/core';
import { CcSnackbarService } from '@common-components/cc-snackbar/cc-snackbar-service';
import { NotificationEventEnum } from '@enums/notification-event-enum';
import { NotificationLevelEnum } from '@enums/notification-level-enum';
import { environment } from '@environments/environment';
import * as signalR from '@microsoft/signalr';
import { Observable, Subject } from 'rxjs';

/**
 * SignalR service for real-time notifications.
 * The hub only notifies: the REST API stays the source of truth, so pages reload their data
 * when an event arrives (and when the connection comes back).
 *
 * USAGE (in a page):
 *   this._signalrService.listen<OrderStatusChangedDTO>(NotificationEventEnum.OrderStatusChanged)
 *     .pipe(takeUntilDestroyed(this._destroyRef))
 *     .subscribe(() => this.loadOrder());
 *   this._signalrService.subscribeToOrder(code);
 */
@Injectable({
  providedIn: 'root',
})
export class SignalrService {
  private readonly _snackbarService: CcSnackbarService = inject(CcSnackbarService);

  private readonly _connection = new signalR.HubConnectionBuilder()
    .withUrl(environment.apiUrls.hub)
    // Never give up while the page is open: quick retries first, then one every 10 seconds
    .withAutomaticReconnect({
      nextRetryDelayInMilliseconds: (context) =>
        [0, 2000, 5000][context.previousRetryCount] ?? 10000,
    })
    .build();

  // one subject per event, created the first time a page listens to it
  private readonly _subjects = new Map<string, Subject<unknown>>();

  // groups joined so far, to join them again after a reconnection
  private readonly _groupJoins = new Map<string, () => Promise<unknown>>();

  private readonly _reconnected = new Subject<void>();
  public readonly reconnected$: Observable<void> = this._reconnected.asObservable();

  constructor() {
    this._connection.onreconnected(async () => {
      // the server forgets the groups when the connection drops
      await Promise.allSettled([...this._groupJoins.values()].map((join) => join()));
      this._reconnected.next();
    });
  }

  public listen<T>(event: NotificationEventEnum): Observable<T> {
    if (!this._subjects.has(event)) {
      const subject = new Subject<unknown>();
      this._subjects.set(event, subject);
      this._connection.on(event, (message: unknown) => subject.next(message));
    }
    return this._subjects.get(event)!.asObservable() as Observable<T>;
  }

  public subscribeToOrder(code: string): Promise<void> {
    return this.joinGroup('SubscribeToOrder', code);
  }

  public joinKitchen(): Promise<void> {
    return this.joinGroup('JoinKitchen');
  }

  // Called when a page is destroyed: the server drops the groups of a closed connection
  public stopConnection(): Promise<void> {
    this._groupJoins.clear();
    return this._connection.stop();
  }

  private async joinGroup(method: string, ...args: string[]): Promise<void> {
    try {
      if (this._connection.state === signalR.HubConnectionState.Disconnected) {
        await this._connection.start();
      }
      await this._connection.invoke(method, ...args);
      this._groupJoins.set(method, () => this._connection.invoke(method, ...args));
    } catch {
      // not blocking: the page still works with the REST API
      this._snackbarService.openSnack(
        NotificationLevelEnum.Error,
        'Aggiornamenti in tempo reale non disponibili.',
      );
    }
  }
}
