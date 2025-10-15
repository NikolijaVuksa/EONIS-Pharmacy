export interface Product {
  id: number;
  name: string;
  rx: boolean;
  basePrice: number;
  vatRate: number;
  manufacturer: string;
  category: string;
  description: string;
  imagePath?: string;
  priceWithVat: number;
  totalStock: number;
}
