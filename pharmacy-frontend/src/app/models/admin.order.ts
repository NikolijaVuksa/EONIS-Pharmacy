export interface OrderItemReadDto {
  id: number;
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  vatRate: number;
}

export interface AdminOrderReadDto {
  id: number;
  status: string;
  customerEmail?: string;
  customerName?: string;
  createdAt: string;
  items: OrderItemReadDto[];
}
