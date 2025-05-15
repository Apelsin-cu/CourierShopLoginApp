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
            
            // You can use the user information to customize the main window
            // For example:
            // this.Title = $"Welcome, {user.FullName}";
            
            // You might also want to set visibility of certain controls based on user role
            // if (user.RoleName == "Admin") { adminPanel.Visibility = Visibility.Visible; }
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
            if (_currentUser == null) return;

            // Пример настройки интерфейса в зависимости от роли пользователя
            Title = $"Courier Shop - {_currentUser.FullName}";

            // Можно настроить видимость элементов в зависимости от роли
            if (!string.IsNullOrEmpty(_currentUser.RoleName))
            {
                switch (_currentUser.RoleName)
                {
                    case "Администратор":
                        // Настройка для администратора
                        break;
                    case "Курьер":
                        // Настройка для курьера
                        break;
                    case "Клиент":
                        // Настройка для клиента
                        break;
                    default:
                        // Настройка по умолчанию
                        break;
                }
            }
        }
    }
}
