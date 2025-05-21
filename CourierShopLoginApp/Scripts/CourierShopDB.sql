-- =============================================
-- SQL скрипт для создания и настройки базы данных CourierShopDB
-- Скрипт включает создание всех таблиц и базовых данных,
-- необходимых для работы приложения CourierShopLoginApp
-- =============================================

-- Проверка существования и создание базы данных
IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = 'CourierShopDB')
BEGIN
    CREATE DATABASE CourierShopDB;
    PRINT 'База данных CourierShopDB создана.';
END
ELSE
    PRINT 'База данных CourierShopDB уже существует.';
GO

USE CourierShopDB;
GO

-- Удаление существующих таблиц (если необходимо пересоздать)
-- Удаляем в порядке, обратном созданию, чтобы не нарушить ограничения внешних ключей
IF OBJECT_ID('Orders', 'U') IS NOT NULL DROP TABLE Orders;
IF OBJECT_ID('Deliveries', 'U') IS NOT NULL DROP TABLE Deliveries;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('OrderStatuses', 'U') IS NOT NULL DROP TABLE OrderStatuses;
IF OBJECT_ID('Roles', 'U') IS NOT NULL DROP TABLE Roles;
GO

-- =============================================
-- Создание таблиц
-- =============================================

-- Таблица ролей
CREATE TABLE Roles (
    role_id INT IDENTITY(1,1) PRIMARY KEY,
    role_name NVARCHAR(50) NOT NULL UNIQUE
);

-- Таблица пользователей
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

-- Таблица статусов заказов
CREATE TABLE OrderStatuses (
    status_id INT IDENTITY(1,1) PRIMARY KEY,
    status_name NVARCHAR(50) NOT NULL UNIQUE
);

-- Таблица заказов
CREATE TABLE Orders (
    order_id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT NULL,
    courier_id INT NULL,
    order_date DATETIME NOT NULL DEFAULT GETDATE(),
    delivery_address NVARCHAR(200) NULL,
    status_id INT NOT NULL DEFAULT 1,
    total_amount DECIMAL(10, 2) NOT NULL DEFAULT 0,
    delivery_date DATETIME NULL,  -- Планируемая дата доставки
    comment NVARCHAR(500) NULL,   -- Комментарий к заказу
    FOREIGN KEY (customer_id) REFERENCES Users(user_id),
    FOREIGN KEY (courier_id) REFERENCES Users(user_id),
    FOREIGN KEY (status_id) REFERENCES OrderStatuses(status_id)
);

-- Таблица доставок (для отслеживания процесса доставки)
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
-- Создание индексов для оптимизации запросов
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
-- Заполнение базовых данных
-- =============================================

-- Заполнение ролей
INSERT INTO Roles (role_name)
SELECT 'Администратор' WHERE NOT EXISTS (SELECT 1 FROM Roles WHERE LOWER(role_name) = LOWER('Администратор'));

INSERT INTO Roles (role_name)
SELECT 'Курьер' WHERE NOT EXISTS (SELECT 1 FROM Roles WHERE LOWER(role_name) = LOWER('Курьер'));

INSERT INTO Roles (role_name)
SELECT 'Клиент' WHERE NOT EXISTS (SELECT 1 FROM Roles WHERE LOWER(role_name) = LOWER('Клиент'));

-- Заполнение статусов заказов
INSERT INTO OrderStatuses (status_name)
SELECT 'Новый' WHERE NOT EXISTS (SELECT 1 FROM OrderStatuses WHERE status_name = 'Новый');

INSERT INTO OrderStatuses (status_name)
SELECT 'В обработке' WHERE NOT EXISTS (SELECT 1 FROM OrderStatuses WHERE status_name = 'В обработке');

INSERT INTO OrderStatuses (status_name)
SELECT 'Доставляется' WHERE NOT EXISTS (SELECT 1 FROM OrderStatuses WHERE status_name = 'Доставляется');

INSERT INTO OrderStatuses (status_name)
SELECT 'Выполнен' WHERE NOT EXISTS (SELECT 1 FROM OrderStatuses WHERE status_name = 'Выполнен');

INSERT INTO OrderStatuses (status_name)
SELECT 'Отменен' WHERE NOT EXISTS (SELECT 1 FROM OrderStatuses WHERE status_name = 'Отменен');

-- Создание тестового администратора (если не существует)
-- Пароль: admin123 (захешированный)
DECLARE @AdminRoleId INT;
SELECT @AdminRoleId = role_id FROM Roles WHERE role_name = 'Администратор';

IF NOT EXISTS (SELECT 1 FROM Users WHERE username = 'admin')
BEGIN
    INSERT INTO Users (username, password_hash, full_name, role_id, is_active)
    VALUES ('admin', 'AQAAAAEAACcQAAAAENUTcffTss8/Q0GUGBfZeprv/8tOU7j6JcCq4y3HuutHvFPXKUcnuKEFfgloCv5Jfg==', 'Администратор системы', @AdminRoleId, 1);
END

-- =============================================
-- Создание тестовых данных для демонстрации
-- =============================================

-- Функция для получения ID роли по имени
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

-- Функция для получения ID статуса по имени
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

-- Создание тестовых курьеров
DECLARE @CourierRoleId INT = dbo.GetRoleId('Курьер');

-- Курьер 1
IF NOT EXISTS (SELECT 1 FROM Users WHERE username = 'courier1')
BEGIN
    INSERT INTO Users (username, password_hash, full_name, phone, role_id, is_active) 
    VALUES ('courier1', 'AQAAAAEAACcQAAAAENUTcffTss8/Q0GUGBfZeprv/8tOU7j6JcCq4y3HuutHvFPXKUcnuKEFfgloCv5Jfg==',
            'Иванов Иван', '+7(900)123-45-67', @CourierRoleId, 1);
END

-- Курьер 2
IF NOT EXISTS (SELECT 1 FROM Users WHERE username = 'courier2')
BEGIN
    INSERT INTO Users (username, password_hash, full_name, phone, role_id, is_active) 
    VALUES ('courier2', 'AQAAAAEAACcQAAAAENUTcffTss8/Q0GUGBfZeprv/8tOU7j6JcCq4y3HuutHvFPXKUcnuKEFfgloCv5Jfg==',
            'Петров Петр', '+7(900)234-56-78', @CourierRoleId, 1);
END

-- Создание тестовых клиентов
DECLARE @ClientRoleId INT = dbo.GetRoleId('Клиент');

-- Клиент 1
IF NOT EXISTS (SELECT 1 FROM Users WHERE username = 'client1')
BEGIN
    INSERT INTO Users (username, password_hash, full_name, phone, email, role_id, is_active) 
    VALUES ('client1', 'AQAAAAEAACcQAAAAENUTcffTss8/Q0GUGBfZeprv/8tOU7j6JcCq4y3HuutHvFPXKUcnuKEFfgloCv5Jfg==',
            'Смирнов Алексей', '+7(900)111-22-33', 'client1@example.com', @ClientRoleId, 1);
END

-- Клиент 2
IF NOT EXISTS (SELECT 1 FROM Users WHERE username = 'client2')
BEGIN
    INSERT INTO Users (username, password_hash, full_name, phone, email, role_id, is_active) 
    VALUES ('client2', 'AQAAAAEAACcQAAAAENUTcffTss8/Q0GUGBfZeprv/8tOU7j6JcCq4y3HuutHvFPXKUcnuKEFfgloCv5Jfg==',
            'Козлова Анна', '+7(900)222-33-44', 'client2@example.com', @ClientRoleId, 1);
END

-- Создание тестовых заказов
DECLARE @ClientId1 INT, @ClientId2 INT;
DECLARE @CourierId1 INT, @CourierId2 INT;
DECLARE @NewStatusId INT = dbo.GetStatusId('Новый');
DECLARE @ProcessingStatusId INT = dbo.GetStatusId('В обработке');
DECLARE @DeliveringStatusId INT = dbo.GetStatusId('Доставляется');
DECLARE @CompletedStatusId INT = dbo.GetStatusId('Выполнен');

SELECT @ClientId1 = user_id FROM Users WHERE username = 'client1';
SELECT @ClientId2 = user_id FROM Users WHERE username = 'client2';
SELECT @CourierId1 = user_id FROM Users WHERE username = 'courier1';
SELECT @CourierId2 = user_id FROM Users WHERE username = 'courier2';

-- Заказ 1 (Новый)
IF NOT EXISTS (SELECT 1 FROM Orders WHERE order_id = 1)
BEGIN
    INSERT INTO Orders (customer_id, order_date, delivery_address, status_id, total_amount, delivery_date, comment)
    VALUES (@ClientId1, DATEADD(DAY, -2, GETDATE()), 'ул. Пушкина, д. 1, кв. 10', @NewStatusId, 1500.00, 
            DATEADD(DAY, 1, GETDATE()), 'Позвонить за час до доставки');
END

-- Заказ 2 (В обработке)
IF NOT EXISTS (SELECT 1 FROM Orders WHERE order_id = 2)
BEGIN
    INSERT INTO Orders (customer_id, order_date, delivery_address, status_id, total_amount, delivery_date)
    VALUES (@ClientId2, DATEADD(DAY, -1, GETDATE()), 'ул. Лермонтова, д. 5, кв. 42', @ProcessingStatusId, 2300.50, 
            DATEADD(DAY, 2, GETDATE()));
END

-- Заказ 3 (Доставляется)
IF NOT EXISTS (SELECT 1 FROM Orders WHERE order_id = 3)
BEGIN
    INSERT INTO Orders (customer_id, courier_id, order_date, delivery_address, status_id, total_amount, delivery_date)
    VALUES (@ClientId1, @CourierId1, DATEADD(DAY, -1, GETDATE()), 'ул. Гоголя, д. 3, кв. 78', @DeliveringStatusId, 990.00, 
            DATEADD(DAY, 0, GETDATE()));
    
    -- Добавляем информацию о доставке
    INSERT INTO Deliveries (order_id, courier_id, assignment_date, start_date, status_id)
    VALUES (3, @CourierId1, DATEADD(HOUR, -3, GETDATE()), DATEADD(HOUR, -1, GETDATE()), @DeliveringStatusId);
END

-- Заказ 4 (Выполнен)
IF NOT EXISTS (SELECT 1 FROM Orders WHERE order_id = 4)
BEGIN
    INSERT INTO Orders (customer_id, courier_id, order_date, delivery_address, status_id, total_amount, delivery_date)
    VALUES (@ClientId2, @CourierId2, DATEADD(DAY, -3, GETDATE()), 'ул. Толстого, д. 7, кв. 15', @CompletedStatusId, 1750.75, 
            DATEADD(DAY, -1, GETDATE()));
    
    -- Добавляем информацию о доставке
    INSERT INTO Deliveries (order_id, courier_id, assignment_date, start_date, completion_date, status_id, comment)
    VALUES (4, @CourierId2, DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, -1, GETDATE()), DATEADD(HOUR, -5, GETDATE()), 
            @CompletedStatusId, 'Доставлено вовремя');
END

-- Заказ 5 (Отменен)
DECLARE @CanceledStatusId INT = dbo.GetStatusId('Отменен');
IF NOT EXISTS (SELECT 1 FROM Orders WHERE order_id = 5)
BEGIN
    INSERT INTO Orders (customer_id, order_date, delivery_address, status_id, total_amount, comment)
    VALUES (@ClientId1, DATEADD(DAY, -4, GETDATE()), 'ул. Чехова, д. 12, кв. 33', @CanceledStatusId, 2100.00, 
            'Отменен клиентом');
END

PRINT 'Скрипт успешно выполнен. База данных CourierShopDB готова к использованию.';
GO

-- Удаляем временные функции
IF OBJECT_ID('GetRoleId', 'FN') IS NOT NULL DROP FUNCTION GetRoleId;
IF OBJECT_ID('GetStatusId', 'FN') IS NOT NULL DROP FUNCTION GetStatusId;
GO