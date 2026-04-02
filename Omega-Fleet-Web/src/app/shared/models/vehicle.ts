export interface Vehicle {
  id: string;
  licensePlate: string;
  manufacturer: string;
  color: string;
  loadCapacityTons?: number | null;
  driverName?: string;
  driverId?: string;
  isActive: boolean
}
