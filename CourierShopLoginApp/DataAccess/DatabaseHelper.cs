using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
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

        public async Task<List<Role>> GetRolesAsync()
        {
            var roles = new List<Role>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT * FROM Roles", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        roles.Add(new Role
                        {
                            RoleId = reader.GetInt32(0),
                            RoleName = reader.GetString(1)
                        });
                    }
                }
            }
            return roles;
        }

        public async Task<bool> CheckUserExistsAsync(string login)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Login = @Login", connection))
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

    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
