export interface Vehicle {
  id: string;
  licensePlate: string;
  manufacturer: string;
  color: string;
  driverName?: string;
  driverId?: string;
  isActive: boolean
}