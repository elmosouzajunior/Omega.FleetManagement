BEGIN TRANSACTION;
ALTER TABLE [expenses] ADD [PricePerLiter] decimal(18,4) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260406140351_AddExpensePricePerLiter', N'9.0.0');

COMMIT;
GO

