using System;
// Keep System.Configuration import but we'll use fully qualified name to be safe

namespace CourierShopLoginApp.Helpers
{
    /// <summary>
    /// Contains global application configuration settings and constants
    /// </summary>
    public static class GlobalConfig
    {
        public static string ConnectionString { get; set; }
        
        public static void InitializeConfig()
        {
            // Initialize application configuration settings
            ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["CourierShopDB"]?.ConnectionString 
                ?? throw new InvalidOperationException("Connection string 'CourierShopDB' not found in configuration.");
        }
    }
}
