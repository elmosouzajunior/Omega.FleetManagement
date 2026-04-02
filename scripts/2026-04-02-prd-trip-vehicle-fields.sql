IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402114139_AddTripTonnageAndVehicleCapacity'
)
BEGIN
    IF COL_LENGTH('vehicles', 'LoadCapacityTons') IS NULL
        ALTER TABLE [vehicles] ADD [LoadCapacityTons] numeric(18,2) NULL;

    IF COL_LENGTH('trips', 'LoadedWeightTons') IS NULL
        ALTER TABLE [trips] ADD [LoadedWeightTons] numeric(18,2) NOT NULL CONSTRAINT [DF_trips_LoadedWeightTons] DEFAULT 0.0;

    IF COL_LENGTH('trips', 'TonValue') IS NULL
        ALTER TABLE [trips] ADD [TonValue] numeric(18,2) NOT NULL CONSTRAINT [DF_trips_TonValue] DEFAULT 0.0;

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260402114139_AddTripTonnageAndVehicleCapacity', N'9.0.0');
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260402121749_AddTripConsumptionAndExpenseLiters'
)
BEGIN
    IF COL_LENGTH('trips', 'ArlaKmPerLiter') IS NULL
        ALTER TABLE [trips] ADD [ArlaKmPerLiter] numeric(18,2) NULL;

    IF COL_LENGTH('trips', 'DieselKmPerLiter') IS NULL
        ALTER TABLE [trips] ADD [DieselKmPerLiter] numeric(18,2) NULL;

    IF COL_LENGTH('expenses', 'Liters') IS NULL
        ALTER TABLE [expenses] ADD [Liters] decimal(18,2) NULL;

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260402121749_AddTripConsumptionAndExpenseLiters', N'9.0.0');
END;
GO

COMMIT;
GO
