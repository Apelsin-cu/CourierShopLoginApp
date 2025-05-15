using System;
using System.Configuration;

namespace CourierShopLoginApp.Config
{
    public static class DatabaseConfig
    {
        private static string _connectionString;

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    // Try to load from app.config/web.config
                    _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;
                    
                    // If not found in config, use a default connection string
                    if (string.IsNullOrEmpty(_connectionString))
                    {
                        _connectionString = "Data Source=(local);Initial Catalog=CourierShopDB;Integrated Security=True";
                    }
                }
                return _connectionString;
            }
        }

        public static void Initialize(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            }
            _connectionString = connectionString;
        }
    }
}
