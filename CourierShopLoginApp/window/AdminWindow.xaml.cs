using CourierShopLoginApp.Helpers;
using CourierShopLoginApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CourierShopLoginApp.window
{
    public partial class AdminWindow : Window
    {
        private DatabaseHelper _db; // Change from readonly to allow reassignment
        private User _currentUser;
        private DispatcherTimer _timer;

        public AdminWindow(User user)
        {
            try
            {
                InitializeComponent(); // Инициализируем компоненты (должен быть первым вызовом)
                
                if (user == null)
                {
                    MessageBox.Show("Ошибка: данные пользователя не были переданы.", 
                        "Ошибка инициализации", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // Убедимся, что GlobalConfig инициализирован перед созданием DatabaseHelper
                GlobalConfig.InitializeConfig();
                
                // Инициализация _db с проверкой на исключения
                try 
                {
                    _db = new DatabaseHelper();
                    System.Diagnostics.Debug.WriteLine("DatabaseHelper initialized successfully");
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error initializing DatabaseHelper: {dbEx.Message}");
                    MessageBox.Show($"Ошибка инициализации базы данных: {dbEx.Message}", 
                        "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }
                
                _currentUser = user;
                
                // Вызываем свойства контролов только после InitializeComponent
                if (AdminNameTextBlock != null)
                {
                    AdminNameTextBlock.Text = $"- {_currentUser?.FullName ?? "Неизвестный пользователь"}";
                }
                
                // Безопасно устанавливаем начальный статус
                UpdateStatus("Инициализация панели администратора...");
                
                // Setup timer for automatic updates
                SetupTimer();
                
                // Load initial data
                Loaded += AdminWindow_Loaded;
                
                Debug.WriteLine("AdminWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AdminWindow constructor: {ex.Message}");
                MessageBox.Show($"Ошибка при инициализации панели администратора: {ex.Message}", 
                    "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update date and time display
            DateTimeTextBlock.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        }

        private async void AdminWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем существование StatusTextBlock
                if (StatusTextBlock == null)
                {
                    Debug.WriteLine("StatusTextBlock is null in AdminWindow_Loaded");
                    MessageBox.Show("Критическая ошибка: элемент StatusTextBlock не найден.", 
                        "Ошибка инициализации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем состояние базы данных перед загрузкой данных
                UpdateStatus("Проверка подключения к базе данных...");
                bool isDatabaseReady = await DatabaseInitializer.EnsureDatabaseStructureAsync();
                if (!isDatabaseReady)
                {
                    UpdateStatus("Ошибка подготовки базы данных. Проверьте подключение.");
                    MessageBox.Show("Не удалось подготовить базу данных. Убедитесь, что сервер доступен и повторите попытку.",
                        "Ошибка подготовки базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Отображаем информацию о процессе загрузки
                UpdateStatus("Загрузка данных...");

                // Загружаем данные с обработкой исключений
                try
                {
                    await LoadDashboardData();
                    UpdateStatus("Загрузка списка пользователей...");
                    await LoadUsers();
                    UpdateStatus("Загрузка заказов...");
                    await LoadOrders();
                    await LoadOrderStatusFilterComboBox();
                    
                    // Set default date range for reports
                    if (StartDatePicker != null && EndDatePicker != null)
                    {
                        StartDatePicker.SelectedDate = DateTime.Now.AddDays(-30);
                        EndDatePicker.SelectedDate = DateTime.Now;
                    }
                    
                    UpdateStatus("Данные загружены успешно");
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Ошибка загрузки данных: {ex.Message}");
                    MessageBox.Show($"Произошла ошибка при загрузке данных: {ex.Message}\n\nАдмин-панель может работать с ограниченной функциональностью.",
                        "Ошибка загрузки данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unhandled error in AdminWindow_Loaded: {ex.Message}");
                MessageBox.Show($"Критическая ошибка при инициализации панели администратора: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для безопасного обновления статуса
        private void UpdateStatus(string message)
        {
            if (StatusTextBlock != null)
            {
                StatusTextBlock.Text = message;
                Debug.WriteLine($"Status updated: {message}");
            }
            else
            {
                Debug.WriteLine($"Unable to update status (StatusTextBlock is null): {message}");
            }
        }

        #region Dashboard Methods

        private async Task LoadDashboardData()
        {
            try
            {
                UpdateStatus("Загрузка данных для дашборда...");
                
                // Get active orders count
                var activeOrders = await _db.GetActiveOrdersCountAsync();
                if (ActiveOrdersCountTextBlock != null)
                    ActiveOrdersCountTextBlock.Text = activeOrders.ToString();
                
                // Get active couriers count
                var activeCouriers = await _db.GetActiveCouriersCountAsync();
                if (ActiveCouriersCountTextBlock != null)
                    ActiveCouriersCountTextBlock.Text = activeCouriers.ToString();
                
                // Get new customers count (today)
                var newCustomers = await _db.GetNewCustomersCountAsync(DateTime.Today);
                if (NewCustomersCountTextBlock != null)
                    NewCustomersCountTextBlock.Text = newCustomers.ToString();
                
                UpdateStatus("Данные дашборда загружены");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки данных дашборда: {ex.Message}");
                // Не пробрасываем исключение дальше, чтобы не прерывать загрузку других компонентов
                MessageBox.Show($"Не удалось загрузить данные для дашборда: {ex.Message}", 
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Users Tab Methods

        private async Task LoadUsers()
        {
            try
            {
                UpdateStatus("Загрузка списка пользователей...");
                
                var users = await _db.GetAllUsersAsync();
                UsersDataGrid.ItemsSource = users;
                if (UsersCountTextBlock != null)
                    UsersCountTextBlock.Text = $"({users.Count})";
                
                UpdateStatus("Список пользователей загружен");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки пользователей: {ex.Message}");
                throw;
            }
        }

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var isUserSelected = UsersDataGrid.SelectedItem != null;
            EditUserButton.IsEnabled = isUserSelected;
            BlockUserButton.IsEnabled = isUserSelected;
            ResetPasswordButton.IsEnabled = isUserSelected;

            if (isUserSelected)
            {
                // Update button text based on user status
                var selectedUser = (User)UsersDataGrid.SelectedItem;
                BlockUserButton.Content = selectedUser.IsActive ? "Блокировать" : "Разблокировать";
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var registrationWindow = new RegistrationWindow();
                registrationWindow.Owner = this;
                registrationWindow.ShowDialog();
                
                // Refresh users list after adding a new user
                RefreshUsersButton_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshUsersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UsersDataGrid.SelectedItem is User selectedUser)
                {
                    // Edit functionality will be implemented here
                    // For now, we'll just display a message
                    MessageBox.Show($"Редактирование пользователя: {selectedUser.FullName}\nID: {selectedUser.UserId}\nФункциональность будет добавлена в будущем.", 
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BlockUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                try
                {
                    // Toggle user's active status
                    bool newStatus = !selectedUser.IsActive;
                    string actionText = newStatus ? "разблокировки" : "блокировки";
                    
                    if (MessageBox.Show($"Вы уверены, что хотите {(newStatus ? "разблокировать" : "заблокировать")} пользователя {selectedUser.FullName}?",
                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        await _db.UpdateUserStatusAsync(selectedUser.UserId, newStatus);
                        selectedUser.IsActive = newStatus;
                        UsersDataGrid.Items.Refresh();
                        
                        // Update button text
                        BlockUserButton.Content = newStatus ? "Блокировать" : "Разблокировать";
                        UpdateStatus($"Пользователь {selectedUser.FullName} успешно {(newStatus ? "разблокирован" : "заблокирован")}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при изменении статуса пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                try
                {
                    if (MessageBox.Show($"Вы уверены, что хотите сбросить пароль для пользователя {selectedUser.FullName}?\nВременный пароль будет сгенерирован и показан вам.",
                        "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        // Generate a random temporary password
                        string tempPassword = GenerateRandomPassword();
                        string hashedPassword = PasswordHelper.HashPassword(tempPassword);
                        
                        // Update password in database
                        await _db.ResetUserPasswordAsync(selectedUser.UserId, hashedPassword);
                        
                        MessageBox.Show($"Пароль для пользователя {selectedUser.FullName} успешно сброшен.\n\nВременный пароль: {tempPassword}\n\nПередайте его пользователю для входа в систему.", 
                            "Пароль сброшен", MessageBoxButton.OK, MessageBoxImage.Information);
                        UpdateStatus($"Пароль для пользователя {selectedUser.FullName} успешно сброшен");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сбросе пароля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var password = new char[8]; // 8 character password
            
            for (int i = 0; i < 8; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            
            return new string(password);
        }

        #endregion

        #region Orders Tab Methods

        private async Task LoadOrders()
        {
            try
            {
                UpdateStatus("Загрузка списка заказов...");
                
                // Check if _db is still null and try to reinitialize it
                if (_db == null)
                {
                    Debug.WriteLine("WARNING: _db is null in LoadOrders, attempting to reinitialize");
                    try
                    {
                        GlobalConfig.InitializeConfig();
                        _db = new DatabaseHelper();
                        Debug.WriteLine("DatabaseHelper reinitialized successfully in LoadOrders");
                    }
                    catch (Exception dbEx)
                    {
                        string errorMessage = $"Ошибка инициализации базы данных: {dbEx.Message}";
                        Debug.WriteLine(errorMessage);
                        UpdateStatus(errorMessage);
                        throw new InvalidOperationException("База данных недоступна", dbEx);
                    }
                }
                
                string selectedStatus = null;
                if (OrderStatusFilterComboBox.SelectedIndex > 0) // Not "All Orders"
                {
                    selectedStatus = (OrderStatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                }
                
                var orders = await _db.GetOrdersAsync(selectedStatus);
                OrdersDataGrid.ItemsSource = orders;
                
                UpdateStatus($"Загружено заказов: {orders.Count}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки заказов: {ex.Message}");
                throw;
            }
        }

        private async Task LoadOrderStatusFilterComboBox()
        {
            // OrderStatusFilterComboBox is already initialized in XAML
            // This method is kept for future dynamic loading of statuses from database
        }

        private void OrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isOrderSelected = OrdersDataGrid.SelectedItem != null;
            ViewOrderDetailsButton.IsEnabled = isOrderSelected;
            AssignCourierButton.IsEnabled = isOrderSelected;
            ChangeOrderStatusButton.IsEnabled = isOrderSelected;
        }

        private async void OrderStatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                await LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении фильтра: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void RefreshOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка заказов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewOrderDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                // Order details view will be implemented in the future
                MessageBox.Show($"Просмотр заказа №{selectedOrder.OrderId}\nКлиент: {selectedOrder.CustomerName}\nАдрес: {selectedOrder.DeliveryAddress}\n\nФункциональность будет добавлена в будущем.", 
                    "Информация о заказе", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void AssignCourierButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                try
                {
                    // In the future, we would display a courier selection dialog
                    // For now, we'll just show a message
                    MessageBox.Show($"Назначение курьера для заказа №{selectedOrder.OrderId}\n\nФункциональность будет добавлена в будущем.", 
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при назначении курьера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ChangeOrderStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                try
                {
                    // In the future, we would display a status selection dialog
                    // For now, we'll just show a message
                    MessageBox.Show($"Изменение статуса заказа №{selectedOrder.OrderId}\nТекущий статус: {selectedOrder.StatusName}\n\nФункциональность будет добавлена в будущем.", 
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при изменении статуса заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Reports Tab Methods

        private async void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ReportTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тип отчета", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                DateTime? startDate = StartDatePicker.SelectedDate;
                DateTime? endDate = EndDatePicker.SelectedDate;
                
                if (!startDate.HasValue || !endDate.HasValue)
                {
                    MessageBox.Show("Выберите начальную и конечную даты для отчета", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                if (startDate > endDate)
                {
                    MessageBox.Show("Начальная дата не может быть позже конечной даты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                UpdateStatus("Формирование отчета...");
                
                string reportType = ((ComboBoxItem)ReportTypeComboBox.SelectedItem).Content.ToString();
                DataTable reportData = null;
                
                switch (reportType)
                {
                    case "Заказы за период":
                        reportData = await _db.GetOrdersReportAsync(startDate.Value, endDate.Value);
                        break;
                    case "Эффективность курьеров":
                        reportData = await _db.GetCourierEfficiencyReportAsync(startDate.Value, endDate.Value);
                        break;
                    case "Популярные адреса доставки":
                        reportData = await _db.GetPopularAddressesReportAsync(startDate.Value, endDate.Value);
                        break;
                    case "Финансовый отчет":
                        reportData = await _db.GetFinancialReportAsync(startDate.Value, endDate.Value);
                        break;
                }
                
                if (reportData != null)
                {
                    ReportDataGrid.ItemsSource = reportData.DefaultView;
                    ReportDataGrid.Visibility = Visibility.Visible;
                    ReportPlaceholderTextBlock.Visibility = Visibility.Collapsed;
                    UpdateStatus($"Отчет \"{reportType}\" сформирован");
                }
                else
                {
                    ReportDataGrid.Visibility = Visibility.Collapsed;
                    ReportPlaceholderTextBlock.Visibility = Visibility.Visible;
                    ReportPlaceholderTextBlock.Text = "Нет данных для отображения";
                    UpdateStatus("Не удалось сформировать отчет");
                }
            }
            catch (Exception ex)
            {
                ReportDataGrid.Visibility = Visibility.Collapsed;
                ReportPlaceholderTextBlock.Visibility = Visibility.Visible;
                UpdateStatus($"Ошибка формирования отчета: {ex.Message}");
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти из системы?", "Подтверждение", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }

        // Cleanup resources
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _timer?.Stop();
        }
    }
}