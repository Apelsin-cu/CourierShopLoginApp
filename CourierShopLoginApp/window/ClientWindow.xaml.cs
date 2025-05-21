using CourierShopLoginApp.Helpers;
using CourierShopLoginApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CourierShopLoginApp.window
{
    public partial class ClientWindow : Window
    {
        private readonly User _currentUser;
        private readonly DatabaseHelper _dbHelper;
        private ObservableCollection<Order> _orders;

        public ClientWindow(User user)
        {
            InitializeComponent();
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            _dbHelper = new DatabaseHelper();
            _orders = new ObservableCollection<Order>();
            
            // ��������� ����������
            UserNameTextBlock.Text = _currentUser.FullName ?? "����������� ������������";
            RoleTextBlock.Text = $"����: {_currentUser.RoleName ?? "������"}";
            
            // �������� ������� ��� ������
            Loaded += ClientWindow_Loaded;
        }

        private async void ClientWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadUserProfile();
                await LoadUserOrders();
                OrdersDataGrid.ItemsSource = _orders;
            }
            catch (Exception ex)
            {
                ShowError($"������ ��� �������� ������: {ex.Message}");
                Debug.WriteLine($"Error loading client data: {ex}");
            }
        }

        private async Task LoadUserProfile()
        {
            try
            {
                using (var connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = @"SELECT username, full_name, phone 
                                     FROM Users 
                                     WHERE user_id = @UserId";
                    
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", _currentUser.UserId);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                UsernameTextBlock.Text = reader["username"].ToString();
                                FullNameProfileTextBlock.Text = reader["full_name"].ToString();
                                PhoneTextBlock.Text = reader["phone"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading user profile: {ex.Message}");
                throw;
            }
        }

        private async Task LoadUserOrders()
        {
            try
            {
                _orders.Clear();
                
                using (var connection = new SqlConnection(GlobalConfig.ConnectionString))
                {
                    await connection.OpenAsync();
                    
                    string query = @"SELECT o.order_id, o.order_date, o.delivery_address, 
                                     o.status_id, s.status_name, o.total_amount,
                                     o.delivery_date, o.comment
                                     FROM Orders o
                                     LEFT JOIN OrderStatuses s ON o.status_id = s.status_id
                                     WHERE o.customer_id = @CustomerId
                                     ORDER BY o.order_date DESC";
                    
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CustomerId", _currentUser.UserId);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var order = new Order
                                {
                                    OrderId = Convert.ToInt32(reader["order_id"]),
                                    OrderDate = Convert.ToDateTime(reader["order_date"]),
                                    DeliveryAddress = reader["delivery_address"].ToString(),
                                    StatusId = Convert.ToInt32(reader["status_id"]),
                                    StatusName = reader["status_name"].ToString(),
                                    TotalAmount = Convert.ToDecimal(reader["total_amount"]),
                                    DeliveryDate = reader["delivery_date"] != DBNull.Value 
                                        ? Convert.ToDateTime(reader["delivery_date"]) 
                                        : (DateTime?)null,
                                    Comment = reader["comment"] != DBNull.Value ? reader["comment"].ToString() : null
                                };
                                
                                _orders.Add(order);
                            }
                        }
                    }
                }
                
                UpdateStatusBar($"��������� �������: {_orders.Count}");
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
                await LoadUserOrders();
            }
            catch (Exception ex)
            {
                ShowError($"������ ��� ���������� ������ �������: {ex.Message}");
            }
        }

        private async void CreateNewOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ������� � ����������� ���� ������ ������
                var newOrderWindow = new NewOrderWindow(_currentUser.UserId)
                {
                    Owner = this, // ������������� ��������� ����
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                // ���������� ���� ��� ��������� ������
                bool? result = newOrderWindow.ShowDialog();
                
                // ���� ����� ��� ������� ������, ��������� ������ �������
                if (result == true)
                {
                    await LoadUserOrders();
                    UpdateStatusBar("����� ������� ������");
                }
            }
            catch (Exception ex)
            {
                ShowError($"������ ��� �������� ������: {ex.Message}");
                Debug.WriteLine($"Error creating order: {ex}");
            }
        }

        private void OrdersDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                try
                {
                    var details = $"����� �{selectedOrder.OrderId}\n" +
                                 $"����: {selectedOrder.OrderDate:dd.MM.yyyy HH:mm}\n" +
                                 $"�����: {selectedOrder.DeliveryAddress}\n" +
                                 $"������: {selectedOrder.StatusName}\n" +
                                 $"�����: {selectedOrder.TotalAmount:N2} ���.";
                    
                    if (selectedOrder.DeliveryDate.HasValue)
                        details += $"\n���� ��������: {selectedOrder.DeliveryDate:dd.MM.yyyy HH:mm}";
                    
                    if (!string.IsNullOrEmpty(selectedOrder.Comment))
                        details += $"\n�����������: {selectedOrder.Comment}";
                    
                    MessageBox.Show(details, "������ ������", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ShowError($"������ ��� �������� ������� ������: {ex.Message}");
                }
            }
        }

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("���������������� ��������� ������� ��������� � ����������.", 
                            "����������", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void UpdateStatusBar(string message)
        {
            StatusTextBlock.Text = message;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "������", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusTextBlock.Text = "������: " + message;
            StatusTextBlock.Foreground = Brushes.Red;
        }
    }
}