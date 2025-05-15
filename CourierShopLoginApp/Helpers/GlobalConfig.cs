using System;

namespace CourierShopLoginApp.Helpers
{
    /// <summary>
    /// Contains global application configuration settings and constants
    /// </summary>
    public static class GlobalConfig
    {
        public static string ConnectionString { get; private set; }
        
        public static void InitializeConfig()
        {
            // Hardcode connection string directly from app.config
            ConnectionString = "Data Source=t1brime-dev.ru;Initial Catalog=CourierShopDB;User ID=Apelsin;Password=gHunjkimjhngbtfder4";
            
            // If connection string is not set, throw an exception
            if (string.IsNullOrEmpty(ConnectionString))
                throw new InvalidOperationException("Connection string not initialized.");
        }
    }
}
