using CourierShopLoginApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CourierShopLoginApp.window
{
    public partial class SelectCourierWindow : Window
    {
        public int? SelectedCourierId { get; private set; }

        public SelectCourierWindow(List<User> couriers, int? currentCourierId = null)
        {
            InitializeComponent();
            
            CouriersListBox.ItemsSource = couriers;
            if (currentCourierId.HasValue)
            {
                var currentCourier = couriers.FirstOrDefault(c => c.UserId == currentCourierId);
                if (currentCourier != null)
                {
                    CouriersListBox.SelectedItem = currentCourier;
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (CouriersListBox.SelectedItem is User selectedCourier)
            {
                SelectedCourierId = selectedCourier.UserId;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Выберите курьера", "Информация", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}