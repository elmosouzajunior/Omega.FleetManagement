export interface OpenTripRequest {
    productId: string;
    clientName: string;
    driverId: string;
    vehicleId: string;
    loadingLocation: string;
    unloadingLocation: string;
    loadingDate: string;
    startKm: number;
    tonValue: number;
    loadedWeightTons: number;
    unloadedWeightTons?: number | null;
    freightValue: number;
    freightAttachment?: File;
}
