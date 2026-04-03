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
    WHERE [MigrationId] = N'20260403103000_ExpandLoadedWeightPrecision'
)
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID(N'[dbo].[trips]')
          AND name = N'LoadedWeightTons'
          AND scale <> 3
    )
    BEGIN
        DECLARE @LoadedWeightDefaultConstraint sysname;

        SELECT @LoadedWeightDefaultConstraint = dc.name
        FROM sys.default_constraints dc
        INNER JOIN sys.columns c
            ON c.default_object_id = dc.object_id
        WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[trips]')
          AND c.name = N'LoadedWeightTons';

        IF @LoadedWeightDefaultConstraint IS NOT NULL
            EXEC(N'ALTER TABLE [dbo].[trips] DROP CONSTRAINT [' + @LoadedWeightDefaultConstraint + ']');

        ALTER TABLE [dbo].[trips] ALTER COLUMN [LoadedWeightTons] numeric(18,3) NOT NULL;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.default_constraints dc
            INNER JOIN sys.columns c
                ON c.default_object_id = dc.object_id
            WHERE dc.parent_object_id = OBJECT_ID(N'[dbo].[trips]')
              AND c.name = N'LoadedWeightTons'
        )
            ALTER TABLE [dbo].[trips]
                ADD CONSTRAINT [DF_trips_LoadedWeightTons] DEFAULT ((0.0)) FOR [LoadedWeightTons];
    END;

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260403103000_ExpandLoadedWeightPrecision', N'9.0.0');
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403150000_AddTripUnloadedWeight'
)
BEGIN
    IF COL_LENGTH('trips', 'UnloadedWeightTons') IS NULL
        ALTER TABLE [dbo].[trips] ADD [UnloadedWeightTons] numeric(18,3) NULL;

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260403150000_AddTripUnloadedWeight', N'9.0.0');
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260403170000_AddDriverCommissions'
)
BEGIN
    IF OBJECT_ID(N'[dbo].[driver_commissions]', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[driver_commissions] (
            [Id] uniqueidentifier NOT NULL,
            [DriverId] uniqueidentifier NOT NULL,
            [Rate] decimal(5,2) NOT NULL,
            CONSTRAINT [pk_driver_commissions] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_driver_commissions_drivers_DriverId]
                FOREIGN KEY ([DriverId]) REFERENCES [dbo].[drivers] ([Id]) ON DELETE CASCADE
        );
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'[dbo].[driver_commissions]')
          AND name = N'IX_driver_commissions_DriverId_Rate'
    )
        CREATE UNIQUE INDEX [IX_driver_commissions_DriverId_Rate]
            ON [dbo].[driver_commissions] ([DriverId], [Rate]);

    INSERT INTO [dbo].[driver_commissions] ([Id], [DriverId], [Rate])
    SELECT NEWID(), d.[Id], ROUND(d.[CommissionRate], 2)
    FROM [dbo].[drivers] d
    WHERE d.[CommissionRate] IS NOT NULL
      AND NOT EXISTS (
          SELECT 1
          FROM [dbo].[driver_commissions] dc
          WHERE dc.[DriverId] = d.[Id]
            AND dc.[Rate] = ROUND(d.[CommissionRate], 2)
      );

    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260403170000_AddDriverCommissions', N'9.0.0');
END;
GO

COMMIT;
GO
