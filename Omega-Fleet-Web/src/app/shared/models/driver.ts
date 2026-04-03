export interface Driver {
  id: string;
  name: string;
  cpf: string;
  commissionRate: number;
  commissionRates: number[];
  isActive: boolean;
}
