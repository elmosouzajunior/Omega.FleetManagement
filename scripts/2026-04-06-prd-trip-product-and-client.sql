BEGIN TRANSACTION;
ALTER TABLE [trips] ADD [ClientName] nvarchar(150) NULL;

ALTER TABLE [trips] ADD [ProductId] uniqueidentifier NULL;

ALTER TABLE [trips] ADD [ProductName] nvarchar(120) NULL;

CREATE TABLE [products] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(300) NULL,
    [IsActive] bit NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_products] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_trips_ProductId] ON [trips] ([ProductId]);

ALTER TABLE [trips] ADD CONSTRAINT [FK_trips_products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [products] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260406130114_AddTripProductAndClient', N'9.0.0');

COMMIT;
GO

