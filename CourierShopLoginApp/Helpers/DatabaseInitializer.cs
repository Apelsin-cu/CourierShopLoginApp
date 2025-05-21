using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace CourierShopLoginApp.Helpers
{
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Проверяет состояние базы данных и создает необходимые таблицы, если они отсутствуют
        /// </summary>
        public static async Task<bool> EnsureDatabaseStructureAsync()
        {
            try
            {
                using (var connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    await connection.OpenAsync();
                    
                    // Проверяем и создаем необходимые таблицы
                    if (!await TableExistsAsync(connection, "Roles"))
                        await CreateRolesTableAsync(connection);
                        
                    if (!await TableExistsAsync(connection, "Users"))
                        await CreateUsersTableAsync(connection);
                        
                    if (!await TableExistsAsync(connection, "OrderStatuses"))
                        await CreateOrderStatusesTableAsync(connection);
                        
                    if (!await TableExistsAsync(connection, "Orders"))
                        await CreateOrdersTableAsync(connection);

                    // Проверяем и создаем базовые данные
                    await EnsureBaseDataAsync(connection);
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        private static async Task<bool> TableExistsAsync(SqlConnection connection, string tableName)
        {
            using (var command = new SqlCommand(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName", connection))
            {
                command.Parameters.AddWithValue("@TableName", tableName);
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
        }
        
        private static async Task CreateRolesTableAsync(SqlConnection connection)
        {
            using (var command = new SqlCommand(@"
                CREATE TABLE Roles (
                    role_id INT IDENTITY(1,1) PRIMARY KEY,
                    role_name NVARCHAR(50) NOT NULL UNIQUE
                )", connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
        
        private static async Task CreateUsersTableAsync(SqlConnection connection)
        {
            using (var command = new SqlCommand(@"
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
                )", connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
        
        private static async Task CreateOrderStatusesTableAsync(SqlConnection connection)
        {
            using (var command = new SqlCommand(@"
                CREATE TABLE OrderStatuses (
                    status_id INT IDENTITY(1,1) PRIMARY KEY,
                    status_name NVARCHAR(50) NOT NULL UNIQUE
                )", connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
        
        private static async Task CreateOrdersTableAsync(SqlConnection connection)
        {
            using (var command = new SqlCommand(@"
                CREATE TABLE Orders (
                    order_id INT IDENTITY(1,1) PRIMARY KEY,
                    customer_id INT NULL,
                    courier_id INT NULL,
                    order_date DATETIME NOT NULL DEFAULT GETDATE(),
                    delivery_address NVARCHAR(200) NULL,
                    status_id INT NOT NULL DEFAULT 1,
                    total_amount DECIMAL(10, 2) NOT NULL DEFAULT 0,
                    FOREIGN KEY (customer_id) REFERENCES Users(user_id),
                    FOREIGN KEY (courier_id) REFERENCES Users(user_id),
                    FOREIGN KEY (status_id) REFERENCES OrderStatuses(status_id)
                )", connection))
            {
                await command.ExecuteNonQueryAsync();
            }
        }
        
        private static async Task EnsureBaseDataAsync(SqlConnection connection)
        {
            // Добавляем базовые роли, если их нет
            if (await GetCountAsync(connection, "Roles") == 0)
            {
                await InsertRoleAsync(connection, "Администратор");
                await InsertRoleAsync(connection, "Курьер");
                await InsertRoleAsync(connection, "Клиент");
            }
            
            // Добавляем базовые статусы заказов, если их нет
            if (await GetCountAsync(connection, "OrderStatuses") == 0)
            {
                await InsertOrderStatusAsync(connection, "Новый");
                await InsertOrderStatusAsync(connection, "В обработке");
                await InsertOrderStatusAsync(connection, "Доставляется");
                await InsertOrderStatusAsync(connection, "Выполнен");
                await InsertOrderStatusAsync(connection, "Отменен");
            }
        }
        
        private static async Task<int> GetCountAsync(SqlConnection connection, string tableName)
        {
            using (var command = new SqlCommand($"SELECT COUNT(*) FROM {tableName}", connection))
            {
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
        }
        
        private static async Task InsertRoleAsync(SqlConnection connection, string roleName)
        {
            // Сначала проверяем, существует ли уже такая роль
            using (var checkCommand = new SqlCommand(
                "SELECT COUNT(*) FROM Roles WHERE LOWER(role_name) = LOWER(@RoleName)", connection))
            {
                checkCommand.Parameters.AddWithValue("@RoleName", roleName);
                int count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                
                if (count == 0)
                {
                    // Роль не существует, добавляем ее
                    using (var command = new SqlCommand(
                        "INSERT INTO Roles (role_name) VALUES (@RoleName)", connection))
                    {
                        command.Parameters.AddWithValue("@RoleName", roleName.Trim());
                        await command.ExecuteNonQueryAsync();
                        
                        // Логируем для отладки
                        Console.WriteLine($"Inserted role: '{roleName}'");
                    }
                }
                else
                {
                    // Роль уже существует, обновляем ее для гарантии правильного написания
                    using (var command = new SqlCommand(
                        "UPDATE Roles SET role_name = @RoleName WHERE LOWER(role_name) = LOWER(@RoleName)", connection))
                    {
                        command.Parameters.AddWithValue("@RoleName", roleName.Trim());
                        await command.ExecuteNonQueryAsync();
                        
                        // Логируем для отладки
                        Console.WriteLine($"Updated role: '{roleName}'");
                    }
                }
            }
        }
        
        private static async Task InsertOrderStatusAsync(SqlConnection connection, string statusName)
        {
            using (var command = new SqlCommand(
                "INSERT INTO OrderStatuses (status_name) VALUES (@StatusName)", connection))
            {
                command.Parameters.AddWithValue("@StatusName", statusName);
                await command.ExecuteNonQueryAsync();
            }
        }

        // Добавить новый метод для проверки содержимого таблицы ролей
        public static async Task<List<string>> GetExistingRolesAsync()
        {
            List<string> roles = new List<string>();
            
            try
            {
                using (var connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT role_name FROM Roles", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                roles.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting roles: {ex.Message}");
            }
            
            return roles;
        }
    }
}