-- =============================================
-- SQL ������ ��� �������� � ��������� ���� ������ CourierShopDB
-- ������ �������� �������� ���� ������ � ������� ������,
-- ����������� ��� ������ ���������� CourierShopLoginApp
-- =============================================

-- �������� ������������� � �������� ���� ������
IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = 'CourierShopDB')
BEGIN
    CREATE DATABASE CourierShopDB;
    PRINT '���� ������ CourierShopDB �������.';
END
ELSE
    PRINT '���� ������ CourierShopDB ��� ����������.';
GO

USE CourierShopDB;
GO

-- �������� ������������ ������ (���� ���������� �����������)
-- ������� � �������, �������� ��������, ����� �� �������� ����������� ������� ������
IF OBJECT_ID('Orders', 'U') IS NOT NULL DROP TABLE Orders;
IF OBJECT_ID('Deliveries', 'U') IS NOT NULL DROP TABLE Deliveries;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('OrderStatuses', 'U') IS NOT NULL DROP TABLE OrderStatuses;
IF OBJECT_ID('Roles', 'U') IS NOT NULL DROP TABLE Roles;
GO

-- =============================================
-- �������� ������
-- =============================================

-- ������� �����
CREATE TABLE Roles (
    role_id INT IDENTITY(1,1) PRIMARY KEY,
    role_name NVARCHAR(50) NOT NULL UNIQUE
);

-- ������� �������������
CREATE TABLE Users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    username NVARCHAR(50) NOT NULL UNIQUE,
    password_hash NVARCHAR(100) NOT NULL,
    full_name NVARCHAR(100) NULL,
    phone NVARCHAR(20) NULL,
    email NVARCHAR(100) NULL,
    role_id INT NULL,
    is_active BIT NOT NULL DEFAULT 1,
    created_date DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (role_id) REFERENCES Roles(role_id)
);

-- ������� �������� �������
CREATE TABLE OrderStatuses (
    status_id INT IDENTITY(1,1) PRIMARY KEY,
    status_name NVARCHAR(50) NOT NULL UNIQUE
);

-- ������� �������
CREATE TABLE Orders (
    order_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT NULL,
    courier_id INT NULL,
    order_date DATETIME NOT NULL DEFAULT GETDATE(),
    delivery_address NVARCHAR(200) NULL,
    status_id INT NOT NULL DEFAULT 1,
    total_amount DECIMAL(10, 2) NOT NULL DEFAULT 0,
    delivery_date DATETIME NULL,  -- ����������� ���� ��������
    comment NVARCHAR(500) NULL,   -- ����������� � ������
    FOREIGN KEY (customer_id) REFERENCES Users(user_id),
    FOREIGN KEY (courier_id) REFERENCES Users(user_id),
    FOREIGN KEY (status_id) REFERENCES OrderStatuses(status_id)
);

-- ������� �������� (��� ������������ �������� ��������)
CREATE TABLE Deliveries (
    delivery_id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL,
    courier_id INT NULL,
    assignment_date DATETIME NOT NULL DEFAULT GETDATE(),
    start_date DATETIME NULL,
    completion_date DATETIME NULL,
    status_id INT NOT NULL,
    comment NVARCHAR(500) NULL,
    FOREIGN KEY (order_id) REFERENCES Orders(order_id),
    FOREIGN KEY (courier_id) REFERENCES Users(user_id),
    FOREIGN KEY (status_id) REFERENCES OrderStatuses(status_id)
);
GO

-- =============================================
-- �������� �������� ��� ����������� ��������
-- =============================================
CREATE INDEX IX_Users_RoleId ON Users(role_id);
CREATE INDEX IX_Orders_CustomerId ON Orders(customer_id);
CREATE INDEX IX_Orders_CourierId ON Orders(courier_id);
CREATE INDEX IX_Orders_StatusId ON Orders(status_id);
CREATE INDEX IX_Deliveries_OrderId ON Deliveries(order_id);
CREATE INDEX IX_Deliveries_CourierId ON Deliveries(courier_id);
CREATE INDEX IX_Deliveries_StatusId ON Deliveries(status_id);
GO

-- =============================================
-- ���������� ������� ������
-- =============================================

-- ���������� �����
INSERT INTO Roles (role_name)
SELECT '�������������' WHERE NOT EXISTS (SELECT 1 FROM Roles WHERE LOWER(role_name) = LOWER('�������������'));

INSERT INTO Roles (role_name)
SELECT '������' WHERE NOT EXISTS (SELECT 1 FROM Roles WHERE LOWER(role_name) = LOWER('������'));

INSERT INTO Roles (role_name)
SELECT '������' WHERE NOT EXISTS (SELECT 1 FROM Roles WHERE LOWER(role_name) = LOWER('������'));

-- ���������� �������� �������
INSERT INTO OrderStatuses (status_name)
SELECT '�����' WHERE NOT EXISTS (SELECT 1 FROM OrderStatuses WHERE status_name = '�����');

INSERT INTO OrderStatuses (status_name)
SELECT '� ���������' WHERE NOT EXISTS (SELECT 1 FROM OrderStatuses WHERE status_name = '� ���������');

INSERT INTO OrderStatuses (status_name)
SELECT '������������' WHERE NOT EXISTS (SELECT 1 FROM OrderStatuses WHERE status_name = '������������');

INSERT INTO OrderStatuses (status_name)
SELECT '��������' WHERE NOT EXISTS (SELECT 1 FROM OrderStatuses WHERE status_name = '��������');

INSERT INTO OrderStatuses (status_name)
SELECT '�������' WHERE NOT EXISTS (SELECT 1 FROM OrderStatuses WHERE status_name = '�������');

-- �������� ��������� �������������� (���� �� ����������)
-- ������: admin123 (��������������)
DECLARE @AdminRoleId INT;
SELECT @AdminRoleId = role_id FROM Roles WHERE role_name = '�������������';

IF NOT EXISTS (SELECT 1 FROM Users WHERE username = 'admin')
BEGIN
    INSERT INTO Users (username, password_hash, full_name, role_id, is_active)
    VALUES ('admin', 'AQAAAAEAACcQAAAAENUTcffTss8/Q0GUGBfZeprv/8tOU7j6JcCq4y3HuutHvFPXKUcnuKEFfgloCv5Jfg==', '������������� �������', @AdminRoleId, 1);
END

-- =============================================
-- �������� �������� ������ ��� ������������
-- =============================================

-- ������� ��� ��������� ID ���� �� �����
IF OBJECT_ID('GetRoleId', 'FN') IS NOT NULL DROP FUNCTION GetRoleId;
GO

CREATE FUNCTION GetRoleId (@roleName NVARCHAR(50))
RETURNS INT
AS
BEGIN
    DECLARE @RoleId INT;
    SELECT @RoleId = role_id FROM Roles WHERE role_name = @roleName;
    RETURN @RoleId;
END;
GO

-- ������� ��� ��������� ID ������� �� �����
IF OBJECT_ID('GetStatusId', 'FN') IS NOT NULL DROP FUNCTION GetStatusId;
GO

CREATE FUNCTION GetStatusId (@statusName NVARCHAR(50))
RETURNS INT
AS
BEGIN
    DECLARE @StatusId INT;
    SELECT @StatusId = status_id FROM OrderStatuses WHERE status_name = @statusName;
    RETURN @StatusId;
END;
GO

-- �������� �������� ��������
DECLARE @CourierRoleId INT = dbo.GetRoleId('������');

-- ������ 1
IF NOT EXISTS (SELECT 1 FROM Users WHERE username = 'courier1')
BEGIN
    INSERT INTO Users (username, password_hash, full_name, phone, role_id, is_active) 
    VALUES ('courier1', 'AQAAAAEAACcQAAAAENUTcffTss8/Q0GUGBfZeprv/8tOU7j6JcCq4y3HuutHvFPXKUcnuKEFfgloCv5Jfg==',
            '������ ����', '+7(900)123-45-67', @CourierRoleId, 1);
END

-- ������ 2
IF NOT EXISTS (SELECT 1 FROM Users WHERE username = 'courier2')
BEGIN
    INSERT INTO Users (username, password_hash, full_name, phone, role_id, is_active) 
    VALUES ('courier2', 'AQAAAAEAACcQAAAAENUTcffTss8/Q0GUGBfZeprv/8tOU7j6JcCq4y3HuutHvFPXKUcnuKEFfgloCv5Jfg==',
            '������ ����', '+7(900)234-56-78', @CourierRoleId, 1);
END

-- �������� �������� ��������
DECLARE @ClientRoleId INT = dbo.GetRoleId('������');

-- ������ 1
IF NOT EXISTS (SELECT 1 FROM Users WHERE username = 'client1')
BEGIN
    INSERT INTO Users (username, password_hash, full_name, phone, email, role_id, is_active) 
    VALUES ('client1', 'AQAAAAEAACcQAAAAENUTcffTss8/Q0GUGBfZeprv/8tOU7j6JcCq4y3HuutHvFPXKUcnuKEFfgloCv5Jfg==',
            '������� �������', '+7(900)111-22-33', 'client1@example.com', @ClientRoleId, 1);
END

-- ������ 2
IF NOT EXISTS (SELECT 1 FROM Users WHERE username = 'client2')
BEGIN
    INSERT INTO Users (username, password_hash, full_name, phone, email, role_id, is_active) 
    VALUES ('client2', 'AQAAAAEAACcQAAAAENUTcffTss8/Q0GUGBfZeprv/8tOU7j6JcCq4y3HuutHvFPXKUcnuKEFfgloCv5Jfg==',
            '������� ����', '+7(900)222-33-44', 'client2@example.com', @ClientRoleId, 1);
END

-- �������� �������� �������
DECLARE @ClientId1 INT, @ClientId2 INT;
DECLARE @CourierId1 INT, @CourierId2 INT;
DECLARE @NewStatusId INT = dbo.GetStatusId('�����');
DECLARE @ProcessingStatusId INT = dbo.GetStatusId('� ���������');
DECLARE @DeliveringStatusId INT = dbo.GetStatusId('������������');
DECLARE @CompletedStatusId INT = dbo.GetStatusId('��������');

SELECT @ClientId1 = user_id FROM Users WHERE username = 'client1';
SELECT @ClientId2 = user_id FROM Users WHERE username = 'client2';
SELECT @CourierId1 = user_id FROM Users WHERE username = 'courier1';
SELECT @CourierId2 = user_id FROM Users WHERE username = 'courier2';

-- ����� 1 (�����)
IF NOT EXISTS (SELECT 1 FROM Orders WHERE order_id = 1)
BEGIN
    INSERT INTO Orders (customer_id, order_date, delivery_address, status_id, total_amount, delivery_date, comment)
    VALUES (@ClientId1, DATEADD(DAY, -2, GETDATE()), '��. �������, �. 1, ��. 10', @NewStatusId, 1500.00, 
            DATEADD(DAY, 1, GETDATE()), '��������� �� ��� �� ��������');
END

-- ����� 2 (� ���������)
IF NOT EXISTS (SELECT 1 FROM Orders WHERE order_id = 2)
BEGIN
    INSERT INTO Orders (customer_id, order_date, delivery_address, status_id, total_amount, delivery_date)
    VALUES (@ClientId2, DATEADD(DAY, -1, GETDATE()), '��. ����������, �. 5, ��. 42', @ProcessingStatusId, 2300.50, 
            DATEADD(DAY, 2, GETDATE()));
END

-- ����� 3 (������������)
IF NOT EXISTS (SELECT 1 FROM Orders WHERE order_id = 3)
BEGIN
    INSERT INTO Orders (customer_id, courier_id, order_date, delivery_address, status_id, total_amount, delivery_date)
    VALUES (@ClientId1, @CourierId1, DATEADD(DAY, -1, GETDATE()), '��. ������, �. 3, ��. 78', @DeliveringStatusId, 990.00, 
            DATEADD(DAY, 0, GETDATE()));
    
    -- ��������� ���������� � ��������
    INSERT INTO Deliveries (order_id, courier_id, assignment_date, start_date, status_id)
    VALUES (3, @CourierId1, DATEADD(HOUR, -3, GETDATE()), DATEADD(HOUR, -1, GETDATE()), @DeliveringStatusId);
END

-- ����� 4 (��������)
IF NOT EXISTS (SELECT 1 FROM Orders WHERE order_id = 4)
BEGIN
    INSERT INTO Orders (customer_id, courier_id, order_date, delivery_address, status_id, total_amount, delivery_date)
    VALUES (@ClientId2, @CourierId2, DATEADD(DAY, -3, GETDATE()), '��. ��������, �. 7, ��. 15', @CompletedStatusId, 1750.75, 
            DATEADD(DAY, -1, GETDATE()));
    
    -- ��������� ���������� � ��������
    INSERT INTO Deliveries (order_id, courier_id, assignment_date, start_date, completion_date, status_id, comment)
    VALUES (4, @CourierId2, DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, -1, GETDATE()), DATEADD(HOUR, -5, GETDATE()), 
            @CompletedStatusId, '���������� �������');
END

-- ����� 5 (�������)
DECLARE @CanceledStatusId INT = dbo.GetStatusId('�������');
IF NOT EXISTS (SELECT 1 FROM Orders WHERE order_id = 5)
BEGIN
    INSERT INTO Orders (customer_id, order_date, delivery_address, status_id, total_amount, comment)
    VALUES (@ClientId1, DATEADD(DAY, -4, GETDATE()), '��. ������, �. 12, ��. 33', @CanceledStatusId, 2100.00, 
            '������� ��������');
END

PRINT '������ ������� ��������. ���� ������ CourierShopDB ������ � �������������.';
GO

-- ������� ��������� �������
IF OBJECT_ID('GetRoleId', 'FN') IS NOT NULL DROP FUNCTION GetRoleId;
IF OBJECT_ID('GetStatusId', 'FN') IS NOT NULL DROP FUNCTION GetStatusId;
GO