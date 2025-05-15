using CourierShopLoginApp.Helpers;
using CourierShopLoginApp.Models; // Add this import for the User class
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Media;

namespace CourierShopLoginApp.window
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        // SQL подключение вместо DbContext
        private SqlConnection _connection;

        public LoginWindow()
        {
            InitializeComponent(); // Make sure this is called first if not already in the code
            
            // Initialize configuration before creating the connection
            GlobalConfig.InitializeConfig();
            
            _connection = new SqlConnection(GlobalConfig.ConnectionString);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                StatusTextBlock.Text = "Имя пользователя и пароль не могут быть пустыми.";
                StatusTextBlock.Foreground = (SolidColorBrush)Application.Current.Resources["ErrorBrush"];
                return;
            }

            try
            {
                // Открываем подключение, если оно закрыто
                if (_connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                }

                // SQL-запрос для поиска пользователя по имени пользователя
                string query = @"SELECT u.user_id, u.username, u.password_hash, u.full_name, u.phone, 
                                u.role_id, r.role_name 
                                FROM Users u 
                                LEFT JOIN Roles r ON u.role_id = r.role_id 
                                WHERE LOWER(u.username) = LOWER(@Username)";

                using (SqlCommand command = new SqlCommand(query, _connection))
                {
                    // Добавляем параметр для защиты от SQL-инъекций
                    command.Parameters.AddWithValue("@Username", username.ToLower());

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Получаем данные пользователя
                            string passwordHash = reader["password_hash"].ToString();

                            if (PasswordHelper.VerifyPassword(password, passwordHash))
                            {
                                StatusTextBlock.Text = "Вход успешен!";
                                StatusTextBlock.Foreground = Brushes.Green;

                                // Получаем данные пользователя напрямую из reader
                                int userId = Convert.ToInt32(reader["user_id"]);
                                string fullName = reader["full_name"] != DBNull.Value ? reader["full_name"].ToString() : null;
                                string roleName = reader["role_name"] != DBNull.Value ? reader["role_name"].ToString() : null;

                                MessageBox.Show($"Добро пожаловать, {fullName}! Роль: {roleName ?? "Не указана"}", 
                                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                                // Создаем объект User и передаем его в главное окно приложения
                                User user = new User { UserId = userId, FullName = fullName, RoleName = roleName };
                                MainWindow mainWindow = new MainWindow(user);
                                mainWindow.Show();
                                this.Close(); // Закрываем окно логина
                            }
                            else
                            {
                                StatusTextBlock.Text = "Неверный пароль.";
                                StatusTextBlock.Foreground = (SolidColorBrush)Application.Current.Resources["ErrorBrush"];
                            }
                        }
                        else
                        {
                            StatusTextBlock.Text = "Пользователь не найден.";
                            StatusTextBlock.Foreground = (SolidColorBrush)Application.Current.Resources["ErrorBrush"];
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                StatusTextBlock.Text = "Ошибка при входе: " + ex.Message;
                StatusTextBlock.Foreground = (SolidColorBrush)Application.Current.Resources["ErrorBrush"];
                // Для отладки: System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                // Закрываем соединение после запроса
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Owner = this; // Необязательно: устанавливает владельца для поведения диалогового окна
            registrationWindow.ShowDialog(); // Показать как модальный диалог
        }

        // Убедитесь, что освобождаете ресурсы соединения при закрытии окна
        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            _connection?.Dispose();
        }
    }
}