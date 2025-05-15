using CourierShopLoginApp.DataModels;
using CourierShopLoginApp.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CourierShopLoginApp.window
{
    public partial class RegistrationWindow : Window
    {
        private readonly Helpers.DatabaseHelper _db;

        public RegistrationWindow()
        {
            GlobalConfig.InitializeConfig(); // Ensure connection string is initialized
            _db = new Helpers.DatabaseHelper();
            InitializeComponent();
            Loaded += RegistrationWindow_Loaded;
        }

        private async void RegistrationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var isConnected = await _db.TestConnectionAsync();
                if (!isConnected)
                {
                    ShowError("Нет подключения к базе данных");
                    return;
                }

                await LoadRoles();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка подключения: {ex.Message}");
            }
        }

        private async Task LoadRoles()
        {
            try
            {
                var rolesTable = await _db.GetRolesAsync();
                RoleComboBox.ItemsSource = rolesTable.DefaultView;
                RoleComboBox.DisplayMemberPath = "RoleName";
                RoleComboBox.SelectedValuePath = "RoleId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}");
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text.Trim();
            var fullName = FullNameTextBox.Text.Trim();
            var phone = PhoneTextBox.Text.Trim();
            var password = PasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;

            if (!ValidateInputs(username, fullName, phone, password, confirmPassword))
                return;

            try
            {
                var loginExists = await ValidateLogin(username);
                if (loginExists)
                {
                    ShowError("Пользователь с таким именем уже существует");
                    return;
                }

                var phoneExists = await _db.CheckPhoneExistsAsync(phone);
                if (phoneExists)
                {
                    ShowError("Пользователь с таким телефоном уже зарегистрирован");
                    return;
                }

                await RegisterUser(username, PasswordHelper.HashPassword(password), (int)RoleComboBox.SelectedValue);

                ShowSuccess("Регистрация прошла успешно!");
                MessageBox.Show("Регистрация завершена. Теперь вы можете войти в систему.",
                                  "Успешная регистрация",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка регистрации: {ex.Message}");
            }
        }

        private async Task<bool> ValidateLogin(string login)
        {
            return await _db.CheckLoginExistsAsync(login);
        }

        private async Task RegisterUser(string login, string password, int roleId)
        {
            await _db.CreateUserAsync(login, password, roleId);
        }

        private bool ValidateInputs(string username, string fullName, string phone, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ShowError("Все поля обязательны для заполнения");
                return false;
            }

            if (password != confirmPassword)
            {
                ShowError("Пароли не совпадают");
                return false;
            }

            if (password.Length < 6)
            {
                ShowError("Пароль должен содержать минимум 6 символов");
                return false;
            }

            if (RoleComboBox.SelectedItem == null)
            {
                ShowError("Не выбрана роль пользователя");
                return false;
            }

            return true;
        }

        private void ShowError(string message)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = (SolidColorBrush)Application.Current.Resources["ErrorBrush"];
        }

        private void ShowSuccess(string message)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = (SolidColorBrush)Application.Current.Resources["SuccessBrush"];
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var isConnected = await _db.TestConnectionAsync();
                if (isConnected)
                {
                    var rolesCount = await _db.GetRolesCountAsync();
                    MessageBox.Show($"Успех! Найдено ролей: {rolesCount}");
                }
                else
                {
                    MessageBox.Show("Ошибка подключения к базе данных");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}