import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { CcSnackbarService } from '@common-components/cc-snackbar/cc-snackbar-service';
import { GetOrderDTO } from '@dtos/order/get-order-dto';
import { NotificationLevelEnum } from '@enums/notification-level-enum';
import { environment } from '@environments/environment';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class KitchenService {
  private readonly _http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrls.backend + '/kitchen';
  private readonly _snackbarService: CcSnackbarService = inject(CcSnackbarService);

  getQueue(): Observable<GetOrderDTO[]> {
    return this._http.get<GetOrderDTO[]>(`${this.apiUrl}/queue`);
  }

  takeNextOrder(): Observable<GetOrderDTO> {
    return this._http.post<GetOrderDTO>(`${this.apiUrl}/queue/next`, null).pipe(
      tap(() => {
        this._snackbarService.openSnack(NotificationLevelEnum.Success, 'Ordine preso in carico.');
      }),
    );
  }

  completeOrder(code: string): Observable<GetOrderDTO> {
    return this._http.post<GetOrderDTO>(`${this.apiUrl}/orders/${code}/complete`, null).pipe(
      tap(() => {
        this._snackbarService.openSnack(NotificationLevelEnum.Success, 'Ordine pronto.');
      }),
    );
  }
}
