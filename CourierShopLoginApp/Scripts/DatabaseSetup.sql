-- Database creation script for CourierShopDB

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CourierShopDB')
BEGIN
    CREATE DATABASE CourierShopDB;
END
GO

USE CourierShopDB;
GO

-- Create Roles table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Roles](
        [role_id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [role_name] [nvarchar](50) NOT NULL,
        [description] [nvarchar](255) NULL
    );
END
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users](
        [user_id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [username] [nvarchar](50) NOT NULL UNIQUE,
        [password_hash] [nvarchar](255) NOT NULL,
        [full_name] [nvarchar](100) NULL,
        [email] [nvarchar](100) NULL,
        [phone] [nvarchar](20) NULL,
        [role_id] [int] NULL,
        [created_date] [datetime] DEFAULT GETDATE(),
        [is_active] [bit] DEFAULT 1,
        CONSTRAINT [FK_Users_Roles] FOREIGN KEY([role_id]) REFERENCES [dbo].[Roles] ([role_id])
    );
END
GO

-- Create Stores table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Stores]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Stores](
        [store_id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [store_name] [nvarchar](100) NOT NULL,
        [address] [nvarchar](255) NULL,
        [phone] [nvarchar](20) NULL,
        [email] [nvarchar](100) NULL
    );
END
GO

-- Create Clients table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Clients](
        [client_id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [full_name] [nvarchar](100) NOT NULL,
        [phone] [nvarchar](20) NULL,
        [email] [nvarchar](100) NULL,
        [address] [nvarchar](255) NULL,
        [registration_date] [datetime] DEFAULT GETDATE()
    );
END
GO

-- Create Orders table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Orders](
        [order_id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [client_id] [int] NOT NULL,
        [store_id] [int] NOT NULL,
        [order_date] [datetime] NOT NULL DEFAULT GETDATE(),
        [delivery_address] [nvarchar](255) NOT NULL,
        [status] [nvarchar](50) NOT NULL DEFAULT 'New',
        [courier_id] [int] NULL, -- References Users table (couriers)
        [total_amount] [decimal](10, 2) NOT NULL DEFAULT 0,
        CONSTRAINT [FK_Orders_Clients] FOREIGN KEY([client_id]) REFERENCES [dbo].[Clients] ([client_id]),
        CONSTRAINT [FK_Orders_Stores] FOREIGN KEY([store_id]) REFERENCES [dbo].[Stores] ([store_id]),
        CONSTRAINT [FK_Orders_Users] FOREIGN KEY([courier_id]) REFERENCES [dbo].[Users] ([user_id])
    );
END
GO

-- Create OrderItems table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderItems]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OrderItems](
        [item_id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [order_id] [int] NOT NULL,
        [item_name] [nvarchar](100) NOT NULL,
        [quantity] [int] NOT NULL DEFAULT 1,
        [price] [decimal](10, 2) NOT NULL,
        CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY([order_id]) REFERENCES [dbo].[Orders] ([order_id])
    );
END
GO

-- Create Payments table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Payments](
        [payment_id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [order_id] [int] NOT NULL,
        [payment_date] [datetime] NOT NULL DEFAULT GETDATE(),
        [payment_method] [nvarchar](50) NOT NULL,
        [amount] [decimal](10, 2) NOT NULL,
        [status] [nvarchar](50) NOT NULL DEFAULT 'Pending',
        CONSTRAINT [FK_Payments_Orders] FOREIGN KEY([order_id]) REFERENCES [dbo].[Orders] ([order_id])
    );
END
GO

-- Create RouteLogs table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RouteLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RouteLogs](
        [log_id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [order_id] [int] NOT NULL,
        [courier_id] [int] NOT NULL,
        [status] [nvarchar](50) NOT NULL,
        [timestamp] [datetime] NOT NULL DEFAULT GETDATE(),
        [comment] [nvarchar](255) NULL,
        CONSTRAINT [FK_RouteLogs_Orders] FOREIGN KEY([order_id]) REFERENCES [dbo].[Orders] ([order_id]),
        CONSTRAINT [FK_RouteLogs_Users] FOREIGN KEY([courier_id]) REFERENCES [dbo].[Users] ([user_id])
    );
END
GO

-- Insert default roles
IF NOT EXISTS (SELECT * FROM [dbo].[Roles] WHERE [role_name] = 'Administrator')
BEGIN
    INSERT INTO [dbo].[Roles] ([role_name], [description])
    VALUES ('Administrator', 'System administrator with full access');
END

IF NOT EXISTS (SELECT * FROM [dbo].[Roles] WHERE [role_name] = 'Manager')
BEGIN
    INSERT INTO [dbo].[Roles] ([role_name], [description])
    VALUES ('Manager', 'Store manager with limited administrative access');
END

IF NOT EXISTS (SELECT * FROM [dbo].[Roles] WHERE [role_name] = 'Courier')
BEGIN
    INSERT INTO [dbo].[Roles] ([role_name], [description])
    VALUES ('Courier', 'Delivery staff');
END

IF NOT EXISTS (SELECT * FROM [dbo].[Roles] WHERE [role_name] = 'Client')
BEGIN
    INSERT INTO [dbo].[Roles] ([role_name], [description])
    VALUES ('Client', 'Customer account');
END
GO

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS [IX_Users_RoleId] ON [dbo].[Users]([role_id]);
CREATE INDEX IF NOT EXISTS [IX_Orders_ClientId] ON [dbo].[Orders]([client_id]);
CREATE INDEX IF NOT EXISTS [IX_Orders_StoreId] ON [dbo].[Orders]([store_id]);
CREATE INDEX IF NOT EXISTS [IX_Orders_CourierId] ON [dbo].[Orders]([courier_id]);
CREATE INDEX IF NOT EXISTS [IX_OrderItems_OrderId] ON [dbo].[OrderItems]([order_id]);
CREATE INDEX IF NOT EXISTS [IX_Payments_OrderId] ON [dbo].[Payments]([order_id]);
CREATE INDEX IF NOT EXISTS [IX_RouteLogs_OrderId] ON [dbo].[RouteLogs]([order_id]);
CREATE INDEX IF NOT EXISTS [IX_RouteLogs_CourierId] ON [dbo].[RouteLogs]([courier_id]);
GO

-- Create Admin user if doesn't exist (password: admin123)
IF NOT EXISTS (SELECT * FROM [dbo].[Users] WHERE [username] = 'admin')
BEGIN
    INSERT INTO [dbo].[Users] ([username], [password_hash], [full_name], [email], [role_id], [is_active])
    VALUES ('admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918', 'System Administrator', 'admin@example.com', 
           (SELECT [role_id] FROM [dbo].[Roles] WHERE [role_name] = 'Administrator'), 1);
END
GO

PRINT 'Database setup completed successfully.';
GO
