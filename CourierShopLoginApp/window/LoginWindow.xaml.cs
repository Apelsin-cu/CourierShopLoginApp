using CourierShopLoginApp.DataModels;
using CourierShopLoginApp.Helpers;
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
using System.Windows.Shapes;

namespace CourierShopLoginApp.window
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        // Замените 'CourierServiceDBEntities' на актуальное имя вашего DbContext
        private CourierShopDBEntities1 _context = new CourierShopDBEntities1();

        public LoginWindow()
        {
            InitializeComponent();
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
                // Поиск пользователя по имени пользователя (обычно без учета регистра для имени пользователя)
                var user = _context.Users.FirstOrDefault(u => u.username.ToLower() == username.ToLower());

                if (user != null)
                {
                    if (PasswordHelper.VerifyPassword(password, user.password_hash))
                    {
                        StatusTextBlock.Text = "Вход успешен!";
                        StatusTextBlock.Foreground = Brushes.Green; // Или используйте кисть из ресурсов

                        MessageBox.Show($"Добро пожаловать, {user.full_name}! Роль: {user.Roles?.role_name ?? "Не указана"}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Открываем главное окно приложения и передаем пользователя
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.CurrentUser = user; // Предполагаем, что в MainWindow есть свойство CurrentUser
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
            catch (System.Exception ex)
            {
                StatusTextBlock.Text = "Ошибка при входе: " + ex.Message;
                StatusTextBlock.Foreground = (SolidColorBrush)Application.Current.Resources["ErrorBrush"];
                // Для отладки: System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Owner = this; // Необязательно: устанавливает владельца для поведения диалогового окна
            registrationWindow.ShowDialog(); // Показать как модальный диалог
        }

        // Убедитесь, что освобождаете контекст при закрытии окна
        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            _context?.Dispose();
        }
    }
}