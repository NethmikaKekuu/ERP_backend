-- Migration 005: Rename PascalCase columns to snake_case to match EF Core ProductDbContext mappings
-- This fixes the "Invalid column name" error caused by a mismatch between the EF Core model
-- (which maps to snake_case) and the database columns (which were created with PascalCase).

-- ============================================================
-- Step 1: Drop duplicate snake_case columns added by migration 003
-- (These were added as extra columns when PascalCase already existed)
-- ============================================================

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'created_by_user_id'
)
AND EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'CreatedByUserId'
)
BEGIN
    ALTER TABLE dbo.products DROP COLUMN created_by_user_id;
END
GO

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'quantity_available'
)
AND EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'QuantityAvailable'
)
BEGIN
    -- Drop any default constraint on the snake_case duplicate first
    DECLARE @dfName NVARCHAR(256);
    SELECT @dfName = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE c.object_id = OBJECT_ID(N'[dbo].[products]') AND c.name = 'quantity_available';
    IF @dfName IS NOT NULL
        EXEC('ALTER TABLE dbo.products DROP CONSTRAINT [' + @dfName + ']');

    ALTER TABLE dbo.products DROP COLUMN quantity_available;
END
GO

-- ============================================================
-- Step 2: Drop foreign keys that reference the columns to rename
-- (sp_rename requires no FK referencing the column being renamed)
-- ============================================================

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Prod_Category')
    ALTER TABLE dbo.products DROP CONSTRAINT FK_Prod_Category;
GO
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Inv_Product')
    ALTER TABLE dbo.inventory DROP CONSTRAINT FK_Inv_Product;
GO
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Res_Product')
    ALTER TABLE dbo.InventoryReservations DROP CONSTRAINT FK_Res_Product;
GO
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Lsa_Product')
    ALTER TABLE dbo.LowStockAlerts DROP CONSTRAINT FK_Lsa_Product;
GO

-- ============================================================
-- Step 3: Rename PascalCase columns in dbo.products to snake_case
-- ============================================================

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'CategoryId')
    EXEC sp_rename 'dbo.products.CategoryId', 'category_id', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'IsActive')
    EXEC sp_rename 'dbo.products.IsActive', 'is_active', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'CreatedAt')
    EXEC sp_rename 'dbo.products.CreatedAt', 'created_at', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'UpdatedAt')
    EXEC sp_rename 'dbo.products.UpdatedAt', 'updated_at', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'CreatedByUserId')
    EXEC sp_rename 'dbo.products.CreatedByUserId', 'created_by_user_id', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'QuantityAvailable')
    EXEC sp_rename 'dbo.products.QuantityAvailable', 'quantity_available', 'COLUMN';
GO

-- ============================================================
-- Step 4: Rename PascalCase columns in dbo.categories to snake_case
-- ============================================================

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[categories]') AND name = 'CreatedAt')
    EXEC sp_rename 'dbo.categories.CreatedAt', 'created_at', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[categories]') AND name = 'UpdatedAt')
    EXEC sp_rename 'dbo.categories.UpdatedAt', 'updated_at', 'COLUMN';
GO

-- ============================================================
-- Step 5: Rename PascalCase columns in dbo.inventory to snake_case
-- (Inventory key column is ProductId matching EF mapping 'product_id')
-- ============================================================

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[inventory]') AND name = 'Id')
    EXEC sp_rename 'dbo.inventory.Id', 'id_old', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[inventory]') AND name = 'ProductId')
    EXEC sp_rename 'dbo.inventory.ProductId', 'product_id', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[inventory]') AND name = 'QuantityAvailable')
    EXEC sp_rename 'dbo.inventory.QuantityAvailable', 'quantity_available', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[inventory]') AND name = 'QuantityReserved')
    EXEC sp_rename 'dbo.inventory.QuantityReserved', 'quantity_reserved', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[inventory]') AND name = 'LowStockThreshold')
    EXEC sp_rename 'dbo.inventory.LowStockThreshold', 'low_stock_threshold', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[inventory]') AND name = 'UpdatedAt')
    EXEC sp_rename 'dbo.inventory.UpdatedAt', 'updated_at', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[inventory]') AND name = 'CreatedAt')
    EXEC sp_rename 'dbo.inventory.CreatedAt', 'created_at', 'COLUMN';
GO

-- ============================================================
-- Step 6: Rename PascalCase columns in dbo.inventory_reservations to snake_case
-- (EF context maps to 'inventory_reservations' table)
-- ============================================================

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[InventoryReservations]') AND name = 'ProductId')
    EXEC sp_rename 'dbo.InventoryReservations.ProductId', 'product_id', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[InventoryReservations]') AND name = 'OrderId')
    EXEC sp_rename 'dbo.InventoryReservations.OrderId', 'order_id', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[InventoryReservations]') AND name = 'Quantity')
    EXEC sp_rename 'dbo.InventoryReservations.Quantity', 'quantity', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[InventoryReservations]') AND name = 'Status')
    EXEC sp_rename 'dbo.InventoryReservations.Status', 'status', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[InventoryReservations]') AND name = 'ReservedAt')
    EXEC sp_rename 'dbo.InventoryReservations.ReservedAt', 'reserved_at', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[InventoryReservations]') AND name = 'ReleasedAt')
    EXEC sp_rename 'dbo.InventoryReservations.ReleasedAt', 'released_at', 'COLUMN';
GO

-- ============================================================
-- Step 7: Rename PascalCase columns in dbo.LowStockAlerts to snake_case
-- ============================================================

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LowStockAlerts]') AND name = 'ProductId')
    EXEC sp_rename 'dbo.LowStockAlerts.ProductId', 'product_id', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LowStockAlerts]') AND name = 'QuantityAtAlert')
    EXEC sp_rename 'dbo.LowStockAlerts.QuantityAtAlert', 'quantity_at_alert', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LowStockAlerts]') AND name = 'IsResolved')
    EXEC sp_rename 'dbo.LowStockAlerts.IsResolved', 'is_resolved', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LowStockAlerts]') AND name = 'AlertedAt')
    EXEC sp_rename 'dbo.LowStockAlerts.AlertedAt', 'created_at', 'COLUMN';
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LowStockAlerts]') AND name = 'ResolvedAt')
    EXEC sp_rename 'dbo.LowStockAlerts.ResolvedAt', 'resolved_at', 'COLUMN';
GO

-- ============================================================
-- Step 8: Re-add foreign keys with snake_case column references
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Prod_Category')
AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[products]') AND name = 'category_id')
BEGIN
    ALTER TABLE dbo.products
    ADD CONSTRAINT FK_Prod_Category
    FOREIGN KEY (category_id) REFERENCES dbo.categories(id);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Inv_Product')
AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[inventory]') AND name = 'product_id')
BEGIN
    ALTER TABLE dbo.inventory
    ADD CONSTRAINT FK_Inv_Product
    FOREIGN KEY (product_id) REFERENCES dbo.products(id) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Res_Product')
AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[InventoryReservations]') AND name = 'product_id')
BEGIN
    ALTER TABLE dbo.InventoryReservations
    ADD CONSTRAINT FK_Res_Product
    FOREIGN KEY (product_id) REFERENCES dbo.products(id) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Lsa_Product')
AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[LowStockAlerts]') AND name = 'product_id')
BEGIN
    ALTER TABLE dbo.LowStockAlerts
    ADD CONSTRAINT FK_Lsa_Product
    FOREIGN KEY (product_id) REFERENCES dbo.products(id) ON DELETE CASCADE;
END
GO
