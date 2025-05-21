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
            
            // Загрузка данных при старте
            Loaded += async (s, e) => await LoadOrderData(orderId);
        }

        private async Task LoadOrderData(int orderId)
        {
            try
            {
                // Загрузка заказа
                _order = await _db.GetOrderDetailsAsync(orderId);
                if (_order == null)
                {
                    MessageBox.Show("Заказ не найден", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // Установка данных в форму
                TitleTextBlock.Text = $"Заказ №{_order.OrderId}";
                OrderDateTextBlock.Text = _order.OrderDate.ToString("dd.MM.yyyy HH:mm");
                TotalAmountTextBlock.Text = $"{_order.TotalAmount:N2} руб.";
                DeliveryAddressTextBox.Text = _order.DeliveryAddress;
                CommentTextBox.Text = _order.Comment;
                CustomerNameTextBlock.Text = $"Клиент: {_order.CustomerName}";
                DeliveryDatePicker.SelectedDate = _order.DeliveryDate;

                // Загрузка статусов
                var statuses = await _db.GetOrderStatusesAsync();
                StatusComboBox.ItemsSource = statuses.DefaultView;
                StatusComboBox.SelectedValue = _order.StatusId;

                // Загрузка курьеров
                if (_isAdmin)
                {
                    var couriers = await _db.GetCouriersAsync();
                    CourierComboBox.ItemsSource = couriers;
                    CourierComboBox.SelectedValue = _order.CourierId;
                }

                // Настройка доступности элементов управления
                ConfigureControlsAccess();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading order data: {ex}");
                ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void ConfigureControlsAccess()
        {
            // Если не администратор - отключаем редактирование
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
                
                MessageBox.Show("Изменения сохранены успешно", "Успех", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving order changes: {ex}");
                ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(DeliveryAddressTextBox.Text))
            {
                ShowError("Введите адрес доставки");
                return false;
            }

            if (StatusComboBox.SelectedItem == null)
            {
                ShowError("Выберите статус заказа");
                return false;
            }

            if (_isAdmin && CourierComboBox.SelectedItem == null && 
                StatusComboBox.SelectedValue.ToString() != "1") // Если статус не "Новый"
            {
                ShowError("Назначьте курьера для заказа");
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