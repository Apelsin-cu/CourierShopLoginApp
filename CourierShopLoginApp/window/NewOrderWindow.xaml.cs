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

            // ������������� ����� �� ���� ������ ��� �������� ����
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
                    StatusId = 1 // ����� �����
                };

                // ������������ ������ �� ����� �������� ������
                var button = (Button)sender;
                button.IsEnabled = false;

                try
                {
                    int orderId = await _db.CreateOrderAsync(order);

                    MessageBox.Show($"����� �{orderId} ������� ������!",
                        "�����", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error creating order: {ex}");
                    ShowError($"������ �������� ������: {ex.Message}");
                    button.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CreateOrderButton_Click: {ex}");
                ShowError($"������: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(DeliveryAddressTextBox.Text))
            {
                ShowError("������� ����� ��������");
                DeliveryAddressTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TotalAmountTextBox.Text))
            {
                ShowError("������� ����� ������");
                TotalAmountTextBox.Focus();
                return false;
            }

            if (!decimal.TryParse(TotalAmountTextBox.Text, out decimal amount))
            {
                ShowError("�������� ������ �����");
                TotalAmountTextBox.Focus();
                return false;
            }

            if (amount <= 0)
            {
                ShowError("����� ������ ������ ���� ������ ����");
                TotalAmountTextBox.Focus();
                return false;
            }

            if (DeliveryAddressTextBox.Text.Length > 200)
            {
                ShowError("����� �������� ������� ������� (�������� 200 ��������)");
                DeliveryAddressTextBox.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(CommentTextBox.Text) && CommentTextBox.Text.Length > 500)
            {
                ShowError("����������� ������� ������� (�������� 500 ��������)");
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
            // ��������� ������ ����� � ���� ���������� �����
            var textBox = sender as TextBox;
            var fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = !_decimalRegex.IsMatch(fullText);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // ��������, ��� ���� ������������ �� ������ ������������� ����
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