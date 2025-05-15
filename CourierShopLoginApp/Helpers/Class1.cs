using System;

namespace CourierShopLoginApp.Helpers
{
    public static class AppConfiguration
    {
        public static string ConnectionString => GlobalConfig.ConnectionString;
        
        public static void EnsureDatabaseConnection()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new InvalidOperationException("Connection string not initialized.");
            }
        }
    }
}
