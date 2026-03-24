export interface OpenTripRequest {
    driverId: string;
    vehicleId: string;
    loadingLocation: string;
    unloadingLocation: string;
    loadingDate: string;
    startKm: number;
    freightValue: number;
    freightAttachment?: File;
}
