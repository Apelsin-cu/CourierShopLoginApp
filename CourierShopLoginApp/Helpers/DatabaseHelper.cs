using CourierShopLoginApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics; // Added missing namespace for Debug class
using System.Threading.Tasks;

namespace CourierShopLoginApp.Helpers
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper()
        {
            try
            {
                // Убедимся, что GlobalConfig инициализирован
                if (string.IsNullOrEmpty(GlobalConfig.ConnectionString))
                {
                    GlobalConfig.InitializeConfig();
                }
                
                _connectionString = GlobalConfig.ConnectionString;
                
                if (string.IsNullOrEmpty(_connectionString))
                {
                    throw new InvalidOperationException("Connection string is empty or not initialized.");
                }
                
                // Проверка соединения при инициализации
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open(); // Проверяем, что можем открыть соединение
                    System.Diagnostics.Debug.WriteLine("DatabaseHelper: Database connection verified");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DatabaseHelper initialization error: {ex.Message}");
                throw new Exception($"Failed to initialize database connection: {ex.Message}", ex);
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CheckLoginExistsAsync(string login)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM Users WHERE username = @Login", connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    return ((int)await command.ExecuteScalarAsync()) > 0;
                }
            }
        }

        public async Task<bool> CheckPhoneExistsAsync(string phone)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM Users WHERE phone = @Phone", connection))
                {
                    command.Parameters.AddWithValue("@Phone", phone);
                    return ((int)await command.ExecuteScalarAsync()) > 0;
                }
            }
        }

        public async Task<DataTable> GetRolesAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT role_id, role_name FROM Roles", connection))
                {
                    var dt = new DataTable();
                    dt.Columns.Add("RoleId", typeof(int));
                    dt.Columns.Add("RoleName", typeof(string));
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            dt.Rows.Add(reader.GetInt32(0), reader.GetString(1));
                        }
                    }
                    return dt;
                }
            }
        }

        public async Task<int> GetRolesCountAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM Roles", connection))
                {
                    return (int)await command.ExecuteScalarAsync();
                }
            }
        }

        public async Task CreateUserAsync(string login, string password, int roleId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "INSERT INTO Users (username, password_hash, role_id, is_active, created_date) VALUES (@Login, @Password, @RoleId, 1, GETDATE())", 
                    connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@RoleId", roleId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Admin functionality methods
        
        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> users = new List<User>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"SELECT u.user_id, u.username, u.password_hash, u.full_name, u.phone, 
                    u.email, u.created_date, u.is_active, u.role_id, r.role_name
                    FROM Users u
                    LEFT JOIN Roles r ON u.role_id = r.role_id
                    ORDER BY u.user_id", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                UserId = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                FullName = !reader.IsDBNull(3) ? reader.GetString(3) : null,
                                Phone = !reader.IsDBNull(4) ? reader.GetString(4) : null,
                                Email = !reader.IsDBNull(5) ? reader.GetString(5) : null,
                                CreatedDate = !reader.IsDBNull(6) ? reader.GetDateTime(6) : (DateTime?)null,
                                IsActive = reader.GetBoolean(7),
                                RoleId = !reader.IsDBNull(8) ? reader.GetInt32(8) : (int?)null,
                                RoleName = !reader.IsDBNull(9) ? reader.GetString(9) : null
                            });
                        }
                    }
                }
            }
            
            return users;
        }

        public async Task UpdateUserStatusAsync(int userId, bool isActive)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "UPDATE Users SET is_active = @IsActive WHERE user_id = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@IsActive", isActive);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task ResetUserPasswordAsync(int userId, string newPasswordHash)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    "UPDATE Users SET password_hash = @PasswordHash WHERE user_id = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> EnsureTableExistsAsync(string tableName)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName", connection))
                    {
                        command.Parameters.AddWithValue("@TableName", tableName);
                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetActiveOrdersCountAsync()
        {
            try
            {
                // Сначала проверяем существование таблиц
                bool ordersTableExists = await EnsureTableExistsAsync("Orders");
                if (!ordersTableExists)
                    return 0;
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        "SELECT COUNT(*) FROM Orders WHERE status_id IN (1, 2, 3)", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        return result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch
            {
                return 0; // В случае ошибки возвращаем 0 вместо исключения
            }
        }

        public async Task<int> GetActiveCouriersCountAsync()
        {
            try
            {
                // Сначала проверяем существование таблиц
                bool usersTableExists = await EnsureTableExistsAsync("Users");
                bool rolesTableExists = await EnsureTableExistsAsync("Roles");
                if (!usersTableExists || !rolesTableExists)
                    return 0;
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        "SELECT COUNT(*) FROM Users u JOIN Roles r ON u.role_id = r.role_id WHERE r.role_name = 'Курьер' AND u.is_active = 1", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        return result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch
            {
                return 0; // В случае ошибки возвращаем 0 вместо исключения
            }
        }

        public async Task<int> GetNewCustomersCountAsync(DateTime date)
        {
            try
            {
                // Сначала проверяем существование таблиц
                bool usersTableExists = await EnsureTableExistsAsync("Users");
                bool rolesTableExists = await EnsureTableExistsAsync("Roles");
                if (!usersTableExists || !rolesTableExists)
                    return 0;
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        @"SELECT COUNT(*) FROM Users u 
                        JOIN Roles r ON u.role_id = r.role_id 
                        WHERE r.role_name = 'Клиент' 
                        AND CONVERT(date, u.created_date) = @Date", connection))
                    {
                        command.Parameters.AddWithValue("@Date", date.Date);
                        var result = await command.ExecuteScalarAsync();
                        return result != DBNull.Value ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch
            {
                return 0; // В случае ошибки возвращаем 0 вместо исключения
            }
        }

        public async Task<List<Order>> GetOrdersAsync(string statusFilter = null)
        {
            List<Order> orders = new List<Order>();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                string query = @"
                    SELECT o.order_id, o.customer_id, o.courier_id, o.order_date, o.delivery_address,
                    o.status_id, os.status_name, c.full_name as customer_name, cu.full_name as courier_name
                    FROM Orders o
                    LEFT JOIN OrderStatuses os ON o.status_id = os.status_id
                    LEFT JOIN Users c ON o.customer_id = c.user_id
                    LEFT JOIN Users cu ON o.courier_id = cu.user_id";
                
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    query += " WHERE os.status_name = @StatusFilter";
                }
                
                query += " ORDER BY o.order_date DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(statusFilter))
                    {
                        command.Parameters.AddWithValue("@StatusFilter", statusFilter);
                    }
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new Order
                            {
                                OrderId = reader.GetInt32(0),
                                CustomerId = !reader.IsDBNull(1) ? reader.GetInt32(1) : (int?)null,
                                CourierId = !reader.IsDBNull(2) ? reader.GetInt32(2) : (int?)null,
                                OrderDate = reader.GetDateTime(3),
                                DeliveryAddress = !reader.IsDBNull(4) ? reader.GetString(4) : null,
                                StatusId = reader.GetInt32(5),
                                StatusName = !reader.IsDBNull(6) ? reader.GetString(6) : null,
                                CustomerName = !reader.IsDBNull(7) ? reader.GetString(7) : null,
                                CourierName = !reader.IsDBNull(8) ? reader.GetString(8) : null
                            });
                        }
                    }
                }
            }
            
            return orders;
        }

        // Report methods
        
        public async Task<DataTable> GetOrdersReportAsync(DateTime startDate, DateTime endDate)
        {
            DataTable reportTable = new DataTable();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"SELECT 
                    o.order_id AS 'Номер заказа', 
                    o.order_date AS 'Дата заказа',
                    c.full_name AS 'Клиент', 
                    o.delivery_address AS 'Адрес доставки',
                    os.status_name AS 'Статус',
                    cu.full_name AS 'Курьер'
                    FROM Orders o
                    LEFT JOIN OrderStatuses os ON o.status_id = os.status_id
                    LEFT JOIN Users c ON o.customer_id = c.user_id
                    LEFT JOIN Users cu ON o.courier_id = cu.user_id
                    WHERE o.order_date BETWEEN @StartDate AND @EndDate
                    ORDER BY o.order_date DESC", connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(reportTable);
                    }
                }
            }
            
            return reportTable;
        }

        public async Task<DataTable> GetCourierEfficiencyReportAsync(DateTime startDate, DateTime endDate)
        {
            DataTable reportTable = new DataTable();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"SELECT 
                    u.full_name AS 'Курьер', 
                    COUNT(o.order_id) AS 'Количество заказов',
                    SUM(CASE WHEN os.status_name = 'Выполнен' THEN 1 ELSE 0 END) AS 'Выполнено',
                    SUM(CASE WHEN os.status_name = 'Отменен' THEN 1 ELSE 0 END) AS 'Отменено',
                    CAST(SUM(CASE WHEN os.status_name = 'Выполнен' THEN 1 ELSE 0 END) * 100.0 / 
                        CASE WHEN COUNT(o.order_id) = 0 THEN 1 ELSE COUNT(o.order_id) END AS DECIMAL(5,2)) AS 'Эффективность (%)'
                    FROM Users u
                    LEFT JOIN Orders o ON u.user_id = o.courier_id AND o.order_date BETWEEN @StartDate AND @EndDate
                    LEFT JOIN OrderStatuses os ON o.status_id = os.status_id
                    JOIN Roles r ON u.role_id = r.role_id
                    WHERE r.role_name = 'Курьер'
                    GROUP BY u.user_id, u.full_name
                    ORDER BY COUNT(o.order_id) DESC", connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(reportTable);
                    }
                }
            }
            
            return reportTable;
        }

        public async Task<DataTable> GetPopularAddressesReportAsync(DateTime startDate, DateTime endDate)
        {
            DataTable reportTable = new DataTable();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"SELECT 
                    delivery_address AS 'Адрес доставки', 
                    COUNT(order_id) AS 'Количество заказов'
                    FROM Orders 
                    WHERE order_date BETWEEN @StartDate AND @EndDate
                    GROUP BY delivery_address
                    ORDER BY COUNT(order_id) DESC", connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(reportTable);
                    }
                }
            }
            
            return reportTable;
        }

        public async Task<DataTable> GetFinancialReportAsync(DateTime startDate, DateTime endDate)
        {
            DataTable reportTable = new DataTable();
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(
                    @"SELECT 
                    CAST(order_date AS DATE) AS 'Дата', 
                    COUNT(order_id) AS 'Количество заказов',
                    SUM(total_amount) AS 'Общая сумма (руб.)'
                    FROM Orders 
                    WHERE order_date BETWEEN @StartDate AND @EndDate
                    GROUP BY CAST(order_date AS DATE)
                    ORDER BY CAST(order_date AS DATE)", connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(reportTable);
                    }
                }
            }
            
            return reportTable;
        }

        // Добавить новый метод для проверки роли пользователя в базе данных
        public async Task<string> GetUserRoleByIdAsync(int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        @"SELECT r.role_name 
                        FROM Users u 
                        JOIN Roles r ON u.role_id = r.role_id 
                        WHERE u.user_id = @UserId", connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        var result = await command.ExecuteScalarAsync();
                        return result != DBNull.Value ? result.ToString() : null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching user role: {ex.Message}");
                return null;
            }
        }

        // Добавить метод для получения списка всех ролей в базе данных
        public async Task<List<string>> GetAllRoleNamesAsync()
        {
            List<string> roleNames = new List<string>();
            
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT role_name FROM Roles", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                roleNames.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting role names: {ex.Message}");
            }
            
            return roleNames;
        }

        // Добавление методов для работы с заказами клиентов

        public async Task<DataTable> GetUserOrdersAsync(int userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    await connection.OpenAsync();

                    string query = @"SELECT o.order_id, o.order_date, o.delivery_address, 
                                   o.status_id, s.status_name, o.total_amount,
                                   o.delivery_date, o.comment
                                   FROM Orders o
                                   LEFT JOIN OrderStatuses s ON o.status_id = s.status_id
                                   WHERE o.customer_id = @CustomerId
                                   ORDER BY o.order_date DESC";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CustomerId", userId);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable ordersTable = new DataTable();
                        adapter.Fill(ordersTable);
                        return ordersTable;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting user orders: {ex.Message}");
                throw;
            }
        }

        public async Task<Order> GetOrderDetailsAsync(int orderId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    await connection.OpenAsync();

                    string query = @"SELECT o.order_id, o.customer_id, o.courier_id, 
                                   o.order_date, o.delivery_address, o.status_id, 
                                   s.status_name, o.total_amount, o.delivery_date, o.comment,
                                   c.full_name as customer_name, cr.full_name as courier_name
                                   FROM Orders o
                                   LEFT JOIN OrderStatuses s ON o.status_id = s.status_id
                                   LEFT JOIN Users c ON o.customer_id = c.user_id
                                   LEFT JOIN Users cr ON o.courier_id = cr.user_id
                                   WHERE o.order_id = @OrderId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Order
                                {
                                    OrderId = reader.GetInt32(reader.GetOrdinal("order_id")),
                                    CustomerId = reader["customer_id"] != DBNull.Value ? (int?)reader.GetInt32(reader.GetOrdinal("customer_id")) : null,
                                    CourierId = reader["courier_id"] != DBNull.Value ? (int?)reader.GetInt32(reader.GetOrdinal("courier_id")) : null,
                                    OrderDate = reader.GetDateTime(reader.GetOrdinal("order_date")),
                                    DeliveryAddress = reader.GetString(reader.GetOrdinal("delivery_address")),
                                    StatusId = reader.GetInt32(reader.GetOrdinal("status_id")),
                                    StatusName = reader.GetString(reader.GetOrdinal("status_name")),
                                    TotalAmount = reader.GetDecimal(reader.GetOrdinal("total_amount")),
                                    DeliveryDate = reader["delivery_date"] != DBNull.Value ? (DateTime?)reader.GetDateTime(reader.GetOrdinal("delivery_date")) : null,
                                    Comment = reader["comment"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("comment")) : null,
                                    CustomerName = reader["customer_name"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("customer_name")) : null,
                                    CourierName = reader["courier_name"] != DBNull.Value ? reader.GetString(reader.GetOrdinal("courier_name")) : null
                                };
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting order details: {ex.Message}");
                throw;
            }
        }

        public async Task<int> CreateOrderAsync(Order order)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    await connection.OpenAsync();

                    string query = @"INSERT INTO Orders 
                                   (customer_id, order_date, delivery_address, status_id, total_amount, comment) 
                                   VALUES 
                                   (@CustomerId, @OrderDate, @DeliveryAddress, @StatusId, @TotalAmount, @Comment);
                                   SELECT SCOPE_IDENTITY();";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CustomerId", order.CustomerId);
                        command.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                        command.Parameters.AddWithValue("@DeliveryAddress", order.DeliveryAddress);
                        command.Parameters.AddWithValue("@StatusId", 1); // 1 = Новый заказ
                        command.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                        command.Parameters.AddWithValue("@Comment", order.Comment ?? (object)DBNull.Value);

                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating order: {ex.Message}");
                throw;
            }
        }

        public async Task<DataTable> GetOrderStatusesAsync()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT status_id, status_name FROM OrderStatuses ORDER BY status_id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable statusesTable = new DataTable();
                        adapter.Fill(statusesTable);
                        return statusesTable;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting order statuses: {ex.Message}");
                throw;
            }
        }

        public async Task<List<User>> GetCouriersAsync()
        {
            var couriers = new List<User>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = @"SELECT u.user_id, u.username, u.full_name, u.phone
                                   FROM Users u
                                   JOIN Roles r ON u.role_id = r.role_id
                                   WHERE r.role_name = N'Курьер'
                                   AND u.is_active = 1
                                   ORDER BY u.full_name";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                couriers.Add(new User
                                {
                                    UserId = reader.GetInt32(0),
                                    Username = reader.GetString(1),
                                    FullName = !reader.IsDBNull(2) ? reader.GetString(2) : null,
                                    Phone = !reader.IsDBNull(3) ? reader.GetString(3) : null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting couriers list: {ex.Message}");
                throw;
            }

            return couriers;
        }

        public async Task UpdateOrderAsync(Order order)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = @"UPDATE Orders 
                                   SET courier_id = @CourierId,
                                       status_id = @StatusId,
                                       delivery_address = @DeliveryAddress,
                                       comment = @Comment,
                                       delivery_date = @DeliveryDate
                                   WHERE order_id = @OrderId";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderId", order.OrderId);
                        command.Parameters.AddWithValue("@CourierId", 
                            (object)order.CourierId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@StatusId", order.StatusId);
                        command.Parameters.AddWithValue("@DeliveryAddress", order.DeliveryAddress);
                        command.Parameters.AddWithValue("@Comment", 
                            (object)order.Comment ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DeliveryDate", 
                            (object)order.DeliveryDate ?? DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating order: {ex.Message}");
                throw;
            }
        }

        // Add this method to DatabaseHelper class
        public async Task<User> GetUserDetailsAsync(int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"SELECT u.user_id, u.username, u.full_name, u.phone, u.email,
                                      r.role_name, u.is_active, u.created_date
                                   FROM Users u
                                   LEFT JOIN Roles r ON u.role_id = r.role_id
                                   WHERE u.user_id = @UserId";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new User
                                {
                                    UserId = userId,
                                    Username = reader["username"].ToString(),
                                    FullName = reader["full_name"] != DBNull.Value ? 
                                             reader["full_name"].ToString() : null,
                                    Phone = reader["phone"] != DBNull.Value ? 
                                           reader["phone"].ToString() : null,
                                    Email = reader["email"] != DBNull.Value ? 
                                           reader["email"].ToString() : null,
                                    RoleName = reader["role_name"] != DBNull.Value ? 
                                             reader["role_name"].ToString() : null,
                                    IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
                                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date"))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting user details: {ex.Message}");
                throw;
            }
            return null;
        }

        public async Task<List<Order>> GetCourierOrdersAsync(int courierId, string statusFilter = null)
        {
            List<Order> orders = new List<Order>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"SELECT o.order_id, o.customer_id, o.courier_id, o.order_date, 
                                   o.delivery_address, o.status_id, os.status_name, 
                                   c.full_name as customer_name, o.total_amount,
                                   o.delivery_date, o.comment
                                   FROM Orders o
                                   LEFT JOIN OrderStatuses os ON o.status_id = os.status_id
                                   LEFT JOIN Users c ON o.customer_id = c.user_id
                                   WHERE o.courier_id = @CourierId";

                    if (!string.IsNullOrEmpty(statusFilter))
                    {
                        query += " AND os.status_name = @StatusFilter";
                    }

                    query += " ORDER BY o.order_date DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CourierId", courierId);
                        if (!string.IsNullOrEmpty(statusFilter))
                        {
                            command.Parameters.AddWithValue("@StatusFilter", statusFilter);
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                orders.Add(new Order
                                {
                                    OrderId = reader.GetInt32(reader.GetOrdinal("order_id")),
                                    CustomerId = reader["customer_id"] != DBNull.Value ? 
                                               (int?)reader.GetInt32(reader.GetOrdinal("customer_id")) : null,
                                    CourierId = reader["courier_id"] != DBNull.Value ? 
                                              (int?)reader.GetInt32(reader.GetOrdinal("courier_id")) : null,
                                    OrderDate = reader.GetDateTime(reader.GetOrdinal("order_date")),
                                    DeliveryAddress = reader.GetString(reader.GetOrdinal("delivery_address")),
                                    StatusId = reader.GetInt32(reader.GetOrdinal("status_id")),
                                    StatusName = reader.GetString(reader.GetOrdinal("status_name")),
                                    CustomerName = reader["customer_name"] != DBNull.Value ? 
                                                reader.GetString(reader.GetOrdinal("customer_name")) : null,
                                    TotalAmount = reader.GetDecimal(reader.GetOrdinal("total_amount")),
                                    DeliveryDate = reader["delivery_date"] != DBNull.Value ? 
                                                 (DateTime?)reader.GetDateTime(reader.GetOrdinal("delivery_date")) : null,
                                    Comment = reader["comment"] != DBNull.Value ? 
                                            reader.GetString(reader.GetOrdinal("comment")) : null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting courier orders: {ex.Message}");
                throw;
            }

            return orders;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, int statusId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"UPDATE Orders 
                                   SET status_id = @StatusId,
                                       delivery_date = CASE 
                                           WHEN @StatusId = 4 THEN GETDATE() -- 4 = Выполнен
                                           ELSE delivery_date 
                                       END
                                   WHERE order_id = @OrderId";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderId", orderId);
                        command.Parameters.AddWithValue("@StatusId", statusId);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating order status: {ex.Message}");
                throw;
            }
        }
    }
}
