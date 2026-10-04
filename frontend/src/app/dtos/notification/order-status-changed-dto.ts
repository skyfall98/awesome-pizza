import { OrderStatusEnum } from '@enums/order-status-enum';

export interface OrderStatusChangedDTO {
  code: string;
  status: OrderStatusEnum;
}
