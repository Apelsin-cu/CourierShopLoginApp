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
                InitializeComponent(); // �������������� ���������� (������ ���� ������ �������)
                
                if (user == null)
                {
                    MessageBox.Show("������: ������ ������������ �� ���� ��������.", 
                        "������ �������������", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // ��������, ��� GlobalConfig ��������������� ����� ��������� DatabaseHelper
                GlobalConfig.InitializeConfig();
                
                // ������������� _db � ��������� �� ����������
                try 
                {
                    _db = new DatabaseHelper();
                    System.Diagnostics.Debug.WriteLine("DatabaseHelper initialized successfully");
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error initializing DatabaseHelper: {dbEx.Message}");
                    MessageBox.Show($"������ ������������� ���� ������: {dbEx.Message}", 
                        "����������� ������", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }
                
                _currentUser = user;
                
                // �������� �������� ��������� ������ ����� InitializeComponent
                if (AdminNameTextBlock != null)
                {
                    AdminNameTextBlock.Text = $"- {_currentUser?.FullName ?? "����������� ������������"}";
                }
                
                // ��������� ������������� ��������� ������
                UpdateStatus("������������� ������ ��������������...");
                
                // Setup timer for automatic updates
                SetupTimer();
                
                // Load initial data
                Loaded += AdminWindow_Loaded;
                
                Debug.WriteLine("AdminWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AdminWindow constructor: {ex.Message}");
                MessageBox.Show($"������ ��� ������������� ������ ��������������: {ex.Message}", 
                    "����������� ������", MessageBoxButton.OK, MessageBoxImage.Error);
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
                // ��������� ������������� StatusTextBlock
                if (StatusTextBlock == null)
                {
                    Debug.WriteLine("StatusTextBlock is null in AdminWindow_Loaded");
                    MessageBox.Show("����������� ������: ������� StatusTextBlock �� ������.", 
                        "������ �������������", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // ��������� ��������� ���� ������ ����� ��������� ������
                UpdateStatus("�������� ����������� � ���� ������...");
                bool isDatabaseReady = await DatabaseInitializer.EnsureDatabaseStructureAsync();
                if (!isDatabaseReady)
                {
                    UpdateStatus("������ ���������� ���� ������. ��������� �����������.");
                    MessageBox.Show("�� ������� ����������� ���� ������. ���������, ��� ������ �������� � ��������� �������.",
                        "������ ���������� ���� ������", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // ���������� ���������� � �������� ��������
                UpdateStatus("�������� ������...");

                // ��������� ������ � ���������� ����������
                try
                {
                    await LoadDashboardData();
                    UpdateStatus("�������� ������ �������������...");
                    await LoadUsers();
                    UpdateStatus("�������� �������...");
                    await LoadOrders();
                    await LoadOrderStatusFilterComboBox();
                    
                    // Set default date range for reports
                    if (StartDatePicker != null && EndDatePicker != null)
                    {
                        StartDatePicker.SelectedDate = DateTime.Now.AddDays(-30);
                        EndDatePicker.SelectedDate = DateTime.Now;
                    }
                    
                    UpdateStatus("������ ��������� �������");
                }
                catch (Exception ex)
                {
                    UpdateStatus($"������ �������� ������: {ex.Message}");
                    MessageBox.Show($"��������� ������ ��� �������� ������: {ex.Message}\n\n�����-������ ����� �������� � ������������ �����������������.",
                        "������ �������� ������", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unhandled error in AdminWindow_Loaded: {ex.Message}");
                MessageBox.Show($"����������� ������ ��� ������������� ������ ��������������: {ex.Message}",
                    "������", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ����� ��� ����������� ���������� �������
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
                UpdateStatus("�������� ������ ��� ��������...");
                
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
                
                UpdateStatus("������ �������� ���������");
            }
            catch (Exception ex)
            {
                UpdateStatus($"������ �������� ������ ��������: {ex.Message}");
                // �� ������������ ���������� ������, ����� �� ��������� �������� ������ �����������
                MessageBox.Show($"�� ������� ��������� ������ ��� ��������: {ex.Message}", 
                    "��������������", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Users Tab Methods

        private async Task LoadUsers()
        {
            try
            {
                UpdateStatus("�������� ������ �������������...");
                
                var users = await _db.GetAllUsersAsync();
                UsersDataGrid.ItemsSource = users;
                if (UsersCountTextBlock != null)
                    UsersCountTextBlock.Text = $"({users.Count})";
                
                UpdateStatus("������ ������������� ��������");
            }
            catch (Exception ex)
            {
                UpdateStatus($"������ �������� �������������: {ex.Message}");
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
                BlockUserButton.Content = selectedUser.IsActive ? "�����������" : "��������������";
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
                MessageBox.Show($"������ ��� �������� ���� �����������: {ex.Message}", "������", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"������ ��� ���������� ������ �������������: {ex.Message}", "������", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show($"�������������� ������������: {selectedUser.FullName}\nID: {selectedUser.UserId}\n���������������� ����� ��������� � �������.", 
                        "����������", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ ��� �������������� ������������: {ex.Message}", "������", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    string actionText = newStatus ? "�������������" : "����������";
                    
                    if (MessageBox.Show($"�� �������, ��� ������ {(newStatus ? "��������������" : "�������������")} ������������ {selectedUser.FullName}?",
                        "�������������", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        await _db.UpdateUserStatusAsync(selectedUser.UserId, newStatus);
                        selectedUser.IsActive = newStatus;
                        UsersDataGrid.Items.Refresh();
                        
                        // Update button text
                        BlockUserButton.Content = newStatus ? "�����������" : "��������������";
                        UpdateStatus($"������������ {selectedUser.FullName} ������� {(newStatus ? "�������������" : "������������")}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"������ ��� ��������� ������� ������������: {ex.Message}", "������", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User selectedUser)
            {
                try
                {
                    if (MessageBox.Show($"�� �������, ��� ������ �������� ������ ��� ������������ {selectedUser.FullName}?\n��������� ������ ����� ������������ � ������� ���.",
                        "�������������", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        // Generate a random temporary password
                        string tempPassword = GenerateRandomPassword();
                        string hashedPassword = PasswordHelper.HashPassword(tempPassword);
                        
                        // Update password in database
                        await _db.ResetUserPasswordAsync(selectedUser.UserId, hashedPassword);
                        
                        MessageBox.Show($"������ ��� ������������ {selectedUser.FullName} ������� �������.\n\n��������� ������: {tempPassword}\n\n��������� ��� ������������ ��� ����� � �������.", 
                            "������ �������", MessageBoxButton.OK, MessageBoxImage.Information);
                        UpdateStatus($"������ ��� ������������ {selectedUser.FullName} ������� �������");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"������ ��� ������ ������: {ex.Message}", "������", MessageBoxButton.OK, MessageBoxImage.Error);
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
                UpdateStatus("�������� ������ �������...");
                
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
                        string errorMessage = $"������ ������������� ���� ������: {dbEx.Message}";
                        Debug.WriteLine(errorMessage);
                        UpdateStatus(errorMessage);
                        throw new InvalidOperationException("���� ������ ����������", dbEx);
                    }
                }
                
                string selectedStatus = null;
                if (OrderStatusFilterComboBox.SelectedIndex > 0) // Not "All Orders"
                {
                    selectedStatus = (OrderStatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                }
                
                var orders = await _db.GetOrdersAsync(selectedStatus);
                OrdersDataGrid.ItemsSource = orders;
                
                UpdateStatus($"��������� �������: {orders.Count}");
            }
            catch (Exception ex)
            {
                UpdateStatus($"������ �������� �������: {ex.Message}");
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
                MessageBox.Show($"������ ��� ���������� �������: {ex.Message}", "������", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"������ ��� ���������� ������ �������: {ex.Message}", "������", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewOrderDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                // Order details view will be implemented in the future
                MessageBox.Show($"�������� ������ �{selectedOrder.OrderId}\n������: {selectedOrder.CustomerName}\n�����: {selectedOrder.DeliveryAddress}\n\n���������������� ����� ��������� � �������.", 
                    "���������� � ������", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    MessageBox.Show($"���������� ������� ��� ������ �{selectedOrder.OrderId}\n\n���������������� ����� ��������� � �������.", 
                        "����������", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"������ ��� ���������� �������: {ex.Message}", "������", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show($"��������� ������� ������ �{selectedOrder.OrderId}\n������� ������: {selectedOrder.StatusName}\n\n���������������� ����� ��������� � �������.", 
                        "����������", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"������ ��� ��������� ������� ������: {ex.Message}", "������", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("�������� ��� ������", "����������", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                DateTime? startDate = StartDatePicker.SelectedDate;
                DateTime? endDate = EndDatePicker.SelectedDate;
                
                if (!startDate.HasValue || !endDate.HasValue)
                {
                    MessageBox.Show("�������� ��������� � �������� ���� ��� ������", "����������", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                if (startDate > endDate)
                {
                    MessageBox.Show("��������� ���� �� ����� ���� ����� �������� ����", "������", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                UpdateStatus("������������ ������...");
                
                string reportType = ((ComboBoxItem)ReportTypeComboBox.SelectedItem).Content.ToString();
                DataTable reportData = null;
                
                switch (reportType)
                {
                    case "������ �� ������":
                        reportData = await _db.GetOrdersReportAsync(startDate.Value, endDate.Value);
                        break;
                    case "������������� ��������":
                        reportData = await _db.GetCourierEfficiencyReportAsync(startDate.Value, endDate.Value);
                        break;
                    case "���������� ������ ��������":
                        reportData = await _db.GetPopularAddressesReportAsync(startDate.Value, endDate.Value);
                        break;
                    case "���������� �����":
                        reportData = await _db.GetFinancialReportAsync(startDate.Value, endDate.Value);
                        break;
                }
                
                if (reportData != null)
                {
                    ReportDataGrid.ItemsSource = reportData.DefaultView;
                    ReportDataGrid.Visibility = Visibility.Visible;
                    ReportPlaceholderTextBlock.Visibility = Visibility.Collapsed;
                    UpdateStatus($"����� \"{reportType}\" �����������");
                }
                else
                {
                    ReportDataGrid.Visibility = Visibility.Collapsed;
                    ReportPlaceholderTextBlock.Visibility = Visibility.Visible;
                    ReportPlaceholderTextBlock.Text = "��� ������ ��� �����������";
                    UpdateStatus("�� ������� ������������ �����");
                }
            }
            catch (Exception ex)
            {
                ReportDataGrid.Visibility = Visibility.Collapsed;
                ReportPlaceholderTextBlock.Visibility = Visibility.Visible;
                UpdateStatus($"������ ������������ ������: {ex.Message}");
                MessageBox.Show($"������ ������������ ������: {ex.Message}", "������", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("�� ������������� ������ ����� �� �������?", "�������������", 
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