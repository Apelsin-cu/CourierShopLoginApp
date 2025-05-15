using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CourierShopLoginApp.Helpers
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper()
        {
            _connectionString = GlobalConfig.ConnectionString;
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
                    "INSERT INTO Users (username, password_hash, role_id) VALUES (@Login, @Password, @RoleId)", 
                    connection))
                {
                    command.Parameters.AddWithValue("@Login", login);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@RoleId", roleId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
