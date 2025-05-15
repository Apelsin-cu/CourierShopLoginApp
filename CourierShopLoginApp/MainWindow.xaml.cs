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

namespace CourierShopLoginApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Users _currentUser;

        public Users CurrentUser
        {
            get { return _currentUser; }
            set 
            { 
                _currentUser = value;
                // Здесь можно настроить интерфейс в зависимости от роли пользователя
                ConfigureUIBasedOnUserRole();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConfigureUIBasedOnUserRole()
        {
            if (_currentUser == null) return;

            // Пример настройки интерфейса в зависимости от роли пользователя
            Title = $"Courier Shop - {_currentUser.full_name}";

            // Можно настроить видимость элементов в зависимости от роли
            if (_currentUser.Roles != null)
            {
                switch (_currentUser.Roles.role_name)
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
