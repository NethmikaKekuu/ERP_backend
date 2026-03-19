-- Create customers table
CREATE TABLE dbo.customers (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    email NVARCHAR(255) NOT NULL UNIQUE,
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    phone NVARCHAR(20),
    created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
 
-- Indexes for customer service
CREATE INDEX idx_customers_email ON dbo.customers(email);
CREATE INDEX idx_customers_created ON dbo.customers(created_at);
