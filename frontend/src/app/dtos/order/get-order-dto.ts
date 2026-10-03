import { OrderStatusEnum } from '@enums/order-status-enum';
import { GetOrderItemDTO } from '@dtos/order-item/get-order-item-dto';

export interface GetOrderDTO {
  code: string;
  customerName: string | null;
  status: OrderStatusEnum;
  createdAt: string;
  totalPrice: number;
  items: GetOrderItemDTO[];
}
