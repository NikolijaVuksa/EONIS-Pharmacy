export interface Product {
  id: number;
  name: string;
  description?: string;
  rx: boolean;
  basePrice: number;
  vatRate: number;
  manufacturer: string;
  category: string;
  priceWithVat: number;
}
