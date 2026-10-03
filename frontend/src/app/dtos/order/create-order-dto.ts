import { CreateOrderItemDTO } from '@dtos/order-item/create-order-item-dto';

export interface CreateOrderDTO {
  customerName?: string | null;
  items: CreateOrderItemDTO[];
}
