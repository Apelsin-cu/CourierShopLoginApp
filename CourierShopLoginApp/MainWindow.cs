using System.Windows;

namespace CourierShopLoginApp
{
    public partial class MainWindow : Window
    {
        public int CurrentUserId { get; private set; }
        
        public MainWindow()
        {
            InitializeComponent();
        }
        
        public MainWindow(int userId)
        {
            InitializeComponent();
            CurrentUserId = userId;
            LoadUserData();
        }
        
        private void LoadUserData()
        {
            // Here you would load user data from database using SqlClient
            // Example:
            // using (var connection = new SqlConnection(GlobalConfig.ConnectionString))
            // {
            //     connection.Open();
            //     using (var command = new SqlCommand("SELECT * FROM Users WHERE user_id = @UserId", connection))
            //     {
            //         command.Parameters.AddWithValue("@UserId", CurrentUserId);
            //         using (var reader = command.ExecuteReader())
            //         {
            //             if (reader.Read())
            //             {
            //                 // Set UI elements based on user data
            //             }
            //         }
            //     }
            // }
        }
    }
}
