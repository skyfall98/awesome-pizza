import { Component, inject } from '@angular/core';
import { MAT_SNACK_BAR_DATA } from '@angular/material/snack-bar';
import { NotificationLevelEnum } from '@enums/notification-level-enum';

@Component({
  selector: 'app-cc-snackbar',
  templateUrl: './cc-snackbar.html',
  styleUrl: './cc-snackbar.scss',
})
export class CcSnackbar {
  protected readonly data: { type: NotificationLevelEnum; message?: string | null } =
    inject(MAT_SNACK_BAR_DATA);
}
