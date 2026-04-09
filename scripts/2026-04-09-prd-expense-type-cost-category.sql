BEGIN TRANSACTION;
ALTER TABLE [expense_types] ADD [CostCategory] int NOT NULL CONSTRAINT [DF_expense_types_CostCategory] DEFAULT 2;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260409154255_AddExpenseTypeCostCategory', N'9.0.0');

COMMIT;
GO
