using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourierShopLoginApp.DataAccess 
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper()
        {
            _connectionString = CourierShopLoginApp.Helpers.GlobalConfig.ConnectionString;
        }

        public async Task<DataTable> GetRolesAsync()
        {
            var dt = new DataTable();
            dt.Columns.Add("RoleId", typeof(int));
            dt.Columns.Add("RoleName", typeof(string));
            
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT role_id, role_name FROM Roles", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        dt.Rows.Add(reader.GetInt32(0), reader.GetString(1));
                    }
                }
            }
            return dt;
        }

        public async Task<bool> CheckUserExistsAsync(string login)
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
    }
}
