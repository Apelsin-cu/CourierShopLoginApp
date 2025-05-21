using CourierShopLoginApp.Models;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CourierShopLoginApp.window
{
    public partial class NewOrderWindow : Window
    {
        private readonly int _customerId;
        private readonly Helpers.DatabaseHelper _db;
        private static readonly Regex _decimalRegex = new Regex(@"^[0-9]*\.?[0-9]*$");

        public NewOrderWindow(int customerId)
        {
            if (customerId <= 0)
                throw new ArgumentException("Invalid customer ID", nameof(customerId));

            InitializeComponent();

            _customerId = customerId;
            _db = new Helpers.DatabaseHelper();

            // Устанавливаем фокус на поле адреса при загрузке окна
            Loaded += (s, e) => DeliveryAddressTextBox.Focus();
        }

        private async void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                var order = new Order
                {
                    CustomerId = _customerId,
                    DeliveryAddress = DeliveryAddressTextBox.Text.Trim(),
                    TotalAmount = decimal.Parse(TotalAmountTextBox.Text),
                    Comment = string.IsNullOrWhiteSpace(CommentTextBox.Text) ? null : CommentTextBox.Text.Trim(),
                    OrderDate = DateTime.Now,
                    StatusId = 1 // Новый заказ
                };

                // Деактивируем кнопку на время создания заказа
                var button = (Button)sender;
                button.IsEnabled = false;

                try
                {
                    int orderId = await _db.CreateOrderAsync(order);

                    MessageBox.Show($"Заказ №{orderId} успешно создан!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error creating order: {ex}");
                    ShowError($"Ошибка создания заказа: {ex.Message}");
                    button.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CreateOrderButton_Click: {ex}");
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(DeliveryAddressTextBox.Text))
            {
                ShowError("Введите адрес доставки");
                DeliveryAddressTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TotalAmountTextBox.Text))
            {
                ShowError("Введите сумму заказа");
                TotalAmountTextBox.Focus();
                return false;
            }

            if (!decimal.TryParse(TotalAmountTextBox.Text, out decimal amount))
            {
                ShowError("Неверный формат суммы");
                TotalAmountTextBox.Focus();
                return false;
            }

            if (amount <= 0)
            {
                ShowError("Сумма заказа должна быть больше нуля");
                TotalAmountTextBox.Focus();
                return false;
            }

            if (DeliveryAddressTextBox.Text.Length > 200)
            {
                ShowError("Адрес доставки слишком длинный (максимум 200 символов)");
                DeliveryAddressTextBox.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(CommentTextBox.Text) && CommentTextBox.Text.Length > 500)
            {
                ShowError("Комментарий слишком длинный (максимум 500 символов)");
                CommentTextBox.Focus();
                return false;
            }

            ErrorTextBlock.Visibility = Visibility.Collapsed;
            return true;
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TotalAmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры и одну десятичную точку
            var textBox = sender as TextBox;
            var fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = !_decimalRegex.IsMatch(fullText);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Убедимся, что окно отображается по центру родительского окна
            if (Owner != null)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
    }
}