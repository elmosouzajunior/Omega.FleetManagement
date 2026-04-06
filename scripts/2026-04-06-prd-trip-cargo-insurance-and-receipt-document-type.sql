BEGIN TRANSACTION;
ALTER TABLE [trips] ADD [CargoInsuranceValue] numeric(18,2) NULL;

ALTER TABLE [trips] ADD [ReceiptDocumentTypeId] uniqueidentifier NULL;

ALTER TABLE [trips] ADD [ReceiptDocumentTypeName] nvarchar(120) NULL;

CREATE TABLE [receipt_document_types] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(300) NULL,
    [IsActive] bit NOT NULL,
    [CompanyId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_receipt_document_types] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_trips_ReceiptDocumentTypeId] ON [trips] ([ReceiptDocumentTypeId]);

ALTER TABLE [trips] ADD CONSTRAINT [FK_trips_receipt_document_types_ReceiptDocumentTypeId] FOREIGN KEY ([ReceiptDocumentTypeId]) REFERENCES [receipt_document_types] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260406142442_AddTripCargoInsuranceAndReceiptDocumentType', N'9.0.0');

COMMIT;
GO

