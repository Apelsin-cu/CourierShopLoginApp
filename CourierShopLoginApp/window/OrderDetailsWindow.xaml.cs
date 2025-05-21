using CourierShopLoginApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CourierShopLoginApp.window
{
    public partial class OrderDetailsWindow : Window
    {
        private readonly Helpers.DatabaseHelper _db;
        private Order _order;
        private readonly bool _isAdmin;
        
        public OrderDetailsWindow(int orderId, bool isAdmin = true)
        {
            InitializeComponent();
            
            _db = new Helpers.DatabaseHelper();
            _isAdmin = isAdmin;
            
            // �������� ������ ��� ������
            Loaded += async (s, e) => await LoadOrderData(orderId);
        }

        private async Task LoadOrderData(int orderId)
        {
            try
            {
                // �������� ������
                _order = await _db.GetOrderDetailsAsync(orderId);
                if (_order == null)
                {
                    MessageBox.Show("����� �� ������", "������", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // ��������� ������ � �����
                TitleTextBlock.Text = $"����� �{_order.OrderId}";
                OrderDateTextBlock.Text = _order.OrderDate.ToString("dd.MM.yyyy HH:mm");
                TotalAmountTextBlock.Text = $"{_order.TotalAmount:N2} ���.";
                DeliveryAddressTextBox.Text = _order.DeliveryAddress;
                CommentTextBox.Text = _order.Comment;
                CustomerNameTextBlock.Text = $"������: {_order.CustomerName}";
                DeliveryDatePicker.SelectedDate = _order.DeliveryDate;

                // �������� ��������
                var statuses = await _db.GetOrderStatusesAsync();
                StatusComboBox.ItemsSource = statuses.DefaultView;
                StatusComboBox.SelectedValue = _order.StatusId;

                // �������� ��������
                if (_isAdmin)
                {
                    var couriers = await _db.GetCouriersAsync();
                    CourierComboBox.ItemsSource = couriers;
                    CourierComboBox.SelectedValue = _order.CourierId;
                }

                // ��������� ����������� ��������� ����������
                ConfigureControlsAccess();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading order data: {ex}");
                ShowError($"������ �������� ������: {ex.Message}");
            }
        }

        private void ConfigureControlsAccess()
        {
            // ���� �� ������������� - ��������� ��������������
            if (!_isAdmin)
            {
                StatusComboBox.IsEnabled = false;
                CourierComboBox.IsEnabled = false;
                DeliveryAddressTextBox.IsReadOnly = true;
                CommentTextBox.IsReadOnly = true;
                DeliveryDatePicker.IsEnabled = false;
            }
        }

        private async void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                var updatedOrder = new Order
                {
                    OrderId = _order.OrderId,
                    CustomerId = _order.CustomerId,
                    CourierId = (int?)CourierComboBox.SelectedValue,
                    StatusId = (int)StatusComboBox.SelectedValue,
                    DeliveryAddress = DeliveryAddressTextBox.Text.Trim(),
                    Comment = string.IsNullOrWhiteSpace(CommentTextBox.Text) ? 
                        null : CommentTextBox.Text.Trim(),
                    DeliveryDate = DeliveryDatePicker.SelectedDate,
                    TotalAmount = _order.TotalAmount,
                    OrderDate = _order.OrderDate
                };

                await _db.UpdateOrderAsync(updatedOrder);
                
                MessageBox.Show("��������� ��������� �������", "�����", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving order changes: {ex}");
                ShowError($"������ ����������: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(DeliveryAddressTextBox.Text))
            {
                ShowError("������� ����� ��������");
                return false;
            }

            if (StatusComboBox.SelectedItem == null)
            {
                ShowError("�������� ������ ������");
                return false;
            }

            if (_isAdmin && CourierComboBox.SelectedItem == null && 
                StatusComboBox.SelectedValue.ToString() != "1") // ���� ������ �� "�����"
            {
                ShowError("��������� ������� ��� ������");
                return false;
            }

            return true;
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}