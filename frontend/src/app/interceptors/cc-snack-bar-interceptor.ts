import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { CcSnackbarService } from '@common-components/cc-snackbar/cc-snackbar-service';
import { NotificationLevelEnum } from '@enums/notification-level-enum';
import { catchError, throwError } from 'rxjs';

export const ccSnackBarInterceptor: HttpInterceptorFn = (req, next) => {
  const _snackService: CcSnackbarService = inject(CcSnackbarService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      switch (error.status) {
        case 400:
        case 404:
        case 409:
          // the backend sends a ProblemDetails with the message in "detail"
          _snackService.openSnack(
            NotificationLevelEnum.Error,
            error.error?.detail ?? 'Richiesta non valida.',
          );
          break;
        default:
          _snackService.openSnack(
            NotificationLevelEnum.Error,
            'Si è verificato un errore imprevisto. Riprova più tardi.',
          );
          break;
      }

      return throwError(() => error);
    }),
  );
};
