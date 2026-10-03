import { inject, Injectable } from '@angular/core';
import {
  MatSnackBar,
  MatSnackBarHorizontalPosition,
  MatSnackBarVerticalPosition,
} from '@angular/material/snack-bar';
import { CcSnackbar } from '@common-components/cc-snackbar/cc-snackbar';
import { NotificationLevelEnum } from '@enums/notification-level-enum';
import { environment } from '@environments/environment';

@Injectable({
  providedIn: 'root',
})
export class CcSnackbarService {
  private readonly _snackBar = inject(MatSnackBar);

  private readonly defaultDuration = environment.snackbar.duration;

  openSnack(
    level: NotificationLevelEnum,
    message?: string | null,
    verticalPosition?: string | null,
    horizontalPosition?: string | null,
    duration?: number | null,
  ) {
    this._snackBar.openFromComponent(CcSnackbar, {
      duration: duration ?? this.defaultDuration,
      panelClass: [NotificationLevelEnum[level].toLowerCase()],
      data: {
        type: level,
        message: message,
      },
      verticalPosition: (verticalPosition ?? 'top') as MatSnackBarVerticalPosition,
      horizontalPosition: (horizontalPosition ?? 'center') as MatSnackBarHorizontalPosition,
    });
  }
}
