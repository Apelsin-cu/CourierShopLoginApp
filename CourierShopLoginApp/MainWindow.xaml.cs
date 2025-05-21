using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CourierShopLoginApp.DataModels;
using CourierShopLoginApp.Models;
using CourierShopLoginApp.window;
using System.Diagnostics;

namespace CourierShopLoginApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User _currentUser;
        private int _userId;

        // Default constructor
        public MainWindow()
        {
            InitializeComponent();
        }

        // Constructor that accepts a User parameter
        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            
            // Initialize UI based on user role
            ConfigureUIBasedOnUserRole();
        }

        // Constructor that accepts a user ID
        public MainWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            
            // You would typically load the user from the database here
            // _currentUser = _userService.GetUserById(userId);
        }

        private void ConfigureUIBasedOnUserRole()
        {
            if (_currentUser == null)
            {
                Debug.WriteLine("ERROR: User is null in ConfigureUIBasedOnUserRole");
                return;
            }

            // Пример настройки интерфейса в зависимости от роли пользователя
            Title = $"Courier Shop - {_currentUser.FullName}";
            Debug.WriteLine($"User role: '{_currentUser.RoleName}'");

            // Можно настроить видимость элементов в зависимости от роли
            if (!string.IsNullOrEmpty(_currentUser.RoleName))
            {
                // Приведение к нижнему регистру для устранения проблем с регистром букв
                string roleName = _currentUser.RoleName.Trim();
                Debug.WriteLine($"Processing role: '{roleName}'");

                if (roleName.Equals("Администратор", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine("Opening Admin Window");
                    OpenAdminWindow();
                }
                else if (roleName.Equals("Курьер", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine("Configuring courier interface");
                    ConfigureCourierInterface();
                }
                else if (roleName.Equals("Клиент", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine("Configuring customer interface");
                    ConfigureCustomerInterface();
                }
                else
                {
                    Debug.WriteLine($"Unknown role: '{roleName}', using default interface");
                    ConfigureDefaultInterface();
                }
            }
            else
            {
                Debug.WriteLine("Role name is null or empty, using default interface");
                ConfigureDefaultInterface();
            }
        }

        private void OpenAdminWindow()
        {
            try
            {
                // Open the admin window and close this window
                AdminWindow adminWindow = new AdminWindow(_currentUser);
                adminWindow.Show();
                Debug.WriteLine("Admin window created and shown successfully");
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening admin window: {ex.Message}");
                MessageBox.Show($"Ошибка при открытии панели администратора: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureCourierInterface()
        {
            try
            {
                // Открываем окно курьера и закрываем текущее окно
                CourierWindow courierWindow = new CourierWindow(_currentUser);
                courierWindow.Show();
                Debug.WriteLine("Courier window created and shown successfully");
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening courier window: {ex.Message}");
                MessageBox.Show($"Ошибка при открытии интерфейса курьера: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureCustomerInterface()
        {
            try
            {
                // Открываем окно клиента и закрываем текущее окно
                ClientWindow clientWindow = new ClientWindow(_currentUser);
                clientWindow.Show();
                Debug.WriteLine("Client window created and shown successfully");
                this.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening client window: {ex.Message}");
                MessageBox.Show($"Ошибка при открытии интерфейса клиента: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigureDefaultInterface()
        {
            // Configure default UI elements
            MessageBox.Show($"Добро пожаловать, {_currentUser.FullName}!\nВаша роль: {_currentUser.RoleName}", 
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
