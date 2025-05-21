using System.Data;
using System.Windows;

namespace CourierShopLoginApp.window
{
    public partial class SelectStatusWindow : Window
    {
        public int SelectedStatusId { get; private set; }

        public SelectStatusWindow(DataTable statuses, int currentStatusId)
        {
            InitializeComponent();
            
            StatusListBox.ItemsSource = statuses.DefaultView;
            foreach (DataRowView row in StatusListBox.Items)
            {
                if ((int)row["StatusId"] == currentStatusId)
                {
                    StatusListBox.SelectedItem = row;
                    break;
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (StatusListBox.SelectedItem is DataRowView selectedStatus)
            {
                SelectedStatusId = (int)selectedStatus["StatusId"];
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Выберите статус", "Информация", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}