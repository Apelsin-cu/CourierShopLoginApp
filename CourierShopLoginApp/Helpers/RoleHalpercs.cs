// CourierShopLoginApp\Helpers\RoleHelper.cs
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace CourierShopLoginApp.Helpers
{
    public static class RoleHelper
    {
        public static async Task FixAdminRoleName()
        {
            try
            {
                using (var connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    await connection.OpenAsync();

                    // Проверяем существование роли "Administrator"
                    int adminRoleId = -1;
                    using (var command = new SqlCommand(
                        "SELECT role_id FROM Roles WHERE role_name = 'Administrator'", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            adminRoleId = Convert.ToInt32(result);
                        }
                    }

                    // Если роль "Administrator" существует, изменяем на "Администратор"
                    if (adminRoleId != -1)
                    {
                        using (var command = new SqlCommand(
                            "UPDATE Roles SET role_name = @NewRoleName WHERE role_id = @RoleId", connection))
                        {
                            command.Parameters.AddWithValue("@NewRoleName", "Администратор");
                            command.Parameters.AddWithValue("@RoleId", adminRoleId);
                            int rowsAffected = await command.ExecuteNonQueryAsync();

                            Debug.WriteLine($"Updated admin role name, rows affected: {rowsAffected}");
                        }
                    }

                    // Выводим все роли для отладки
                    using (var command = new SqlCommand("SELECT role_id, role_name FROM Roles", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            Debug.WriteLine("Current roles in database:");
                            while (await reader.ReadAsync())
                            {
                                int roleId = reader.GetInt32(0);
                                string roleName = reader.GetString(1);
                                Debug.WriteLine($"  {roleId}: '{roleName}'");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fixing admin role: {ex.Message}");
                MessageBox.Show($"Ошибка исправления роли администратора: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}