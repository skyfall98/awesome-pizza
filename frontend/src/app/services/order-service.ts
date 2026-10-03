import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { CcSnackbarService } from '@common-components/cc-snackbar/cc-snackbar-service';
import { CreateOrderDTO } from '@dtos/order/create-order-dto';
import { GetOrderDTO } from '@dtos/order/get-order-dto';
import { NotificationLevelEnum } from '@enums/notification-level-enum';
import { environment } from '@environments/environment';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private readonly _http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrls.backend + '/order';
  private readonly _snackbarService: CcSnackbarService = inject(CcSnackbarService);

  createOrder(dto: CreateOrderDTO): Observable<GetOrderDTO> {
    return this._http.post<GetOrderDTO>(this.apiUrl, dto).pipe(
      tap(() => {
        this._snackbarService.openSnack(NotificationLevelEnum.Success, 'Ordine inviato!');
      }),
    );
  }

  getOrderByCode(code: string): Observable<GetOrderDTO> {
    return this._http.get<GetOrderDTO>(`${this.apiUrl}/${code}`);
  }
}
