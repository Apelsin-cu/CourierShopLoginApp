using CourierShopLoginApp.Helpers;
using CourierShopLoginApp.Models;
using System;
using System.Collections.Generic; // Add this line
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;      // Add this line
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;       // Add this line
using System.Windows.Media;

namespace CourierShopLoginApp.window
{
    public partial class CourierWindow : Window
    {
        private readonly User _currentUser;
        private readonly DatabaseHelper _dbHelper;
        private ObservableCollection<Order> _orders;

        public CourierWindow(User user)
        {
            InitializeComponent();
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            _dbHelper = new DatabaseHelper();
            _orders = new ObservableCollection<Order>();

            // Настройка интерфейса
            UserNameTextBlock.Text = _currentUser.FullName ?? "Неизвестный курьер";
            RoleTextBlock.Text = $"Роль: {_currentUser.RoleName ?? "Курьер"}";

            // Загрузка данных при старте
            Loaded += CourierWindow_Loaded;
        }

        private async void CourierWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadUserProfile();
                await LoadOrderStatuses();
                await LoadCourierOrders();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при загрузке данных: {ex.Message}");
                Debug.WriteLine($"Error loading courier data: {ex}");
            }
        }

        private async Task LoadUserProfile()
        {
            try
            {
                var userDetails = await _dbHelper.GetUserDetailsAsync(_currentUser.UserId);
                if (userDetails != null)
                {
                    FullNameTextBlock.Text = userDetails.FullName;
                    PhoneTextBlock.Text = userDetails.Phone;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading user profile: {ex.Message}");
                throw;
            }
        }

        private async Task LoadOrderStatuses()
        {
            try
            {
                var statuses = await _dbHelper.GetOrderStatusesAsync();
                
                // Add "Все статусы" option at the beginning
                var allStatusesTable = new DataTable();
                allStatusesTable.Columns.Add("StatusId", typeof(int));
                allStatusesTable.Columns.Add("StatusName", typeof(string));
                
                // Добавляем опцию "Все статусы"
                allStatusesTable.Rows.Add(-1, "Все статусы");

                // Копируем существующие статусы
                foreach (DataRow row in statuses.Rows)
                {
                    allStatusesTable.Rows.Add(
                        Convert.ToInt32(row["status_id"]),
                        Convert.ToString(row["status_name"])
                    );
                }

                StatusFilterComboBox.ItemsSource = null; // Clear the existing items
                StatusFilterComboBox.Items.Clear();      // Ensure complete cleanup
                StatusFilterComboBox.ItemsSource = allStatusesTable.DefaultView;
                StatusFilterComboBox.SelectedIndex = 0;  // Select "Все статусы" by default

                Debug.WriteLine($"Loaded {allStatusesTable.Rows.Count} statuses");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading order statuses: {ex.Message}");
                throw;
            }
        }

        private async Task LoadCourierOrders()
        {
            try
            {
                _orders.Clear();
                string statusFilter = null;

                // Get selected status
                if (StatusFilterComboBox.SelectedValue != null && 
                    (int)StatusFilterComboBox.SelectedValue != -1)
                {
                    var selectedRow = ((DataRowView)StatusFilterComboBox.SelectedItem).Row;
                    statusFilter = selectedRow["StatusName"].ToString();
                }

                var orders = await _dbHelper.GetCourierOrdersAsync(_currentUser.UserId, statusFilter);
                foreach (var order in orders)
                {
                    _orders.Add(order);
                }

                OrdersDataGrid.ItemsSource = _orders;
                UpdateStatusBar($"Загружено заказов: {_orders.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading orders: {ex.Message}");
                throw;
            }
        }

        private async void RefreshOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadCourierOrders();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при обновлении списка заказов: {ex.Message}");
            }
        }

        private async void OrdersDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                try
                {
                    var detailsWindow = new OrderDetailsWindow(selectedOrder.OrderId, false);
                    detailsWindow.Owner = this;
                    if (detailsWindow.ShowDialog() == true)
                    {
                        await LoadCourierOrders();
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка при открытии деталей заказа: {ex.Message}");
                }
            }
        }

        private async void ViewOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var order = button?.DataContext as Order;
            if (order == null) return;

            try
            {
                var detailsWindow = new OrderDetailsWindow(order.OrderId, false);
                detailsWindow.Owner = this;
                if (detailsWindow.ShowDialog() == true)
                {
                    await LoadCourierOrders();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при открытии деталей заказа: {ex.Message}");
            }
        }

        private async void ChangeStatus_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var order = button?.DataContext as Order;
            if (order == null) return;

            try
            {
                var statuses = await _dbHelper.GetOrderStatusesAsync();
                var statusWindow = new SelectStatusWindow(statuses, order.StatusId);
                statusWindow.Owner = this;

                if (statusWindow.ShowDialog() == true)
                {
                    await _dbHelper.UpdateOrderStatusAsync(order.OrderId, statusWindow.SelectedStatusId);
                    await LoadCourierOrders();
                    UpdateStatusBar("Статус заказа успешно обновлен");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при изменении статуса заказа: {ex.Message}");
            }
        }

        private async void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusFilterComboBox.SelectedItem != null)
            {
                try
                {
                    await LoadCourierOrders();
                }
                catch (Exception ex)
                {
                    ShowError($"Ошибка при фильтрации заказов: {ex.Message}");
                }
            }
        }

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция изменения профиля находится в разработке",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void UpdateStatusBar(string message)
        {
            StatusTextBlock.Text = message;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusTextBlock.Text = "Ошибка: " + message;
            StatusTextBlock.Foreground = Brushes.Red;
        }
    }
}