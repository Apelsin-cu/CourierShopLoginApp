using System;

namespace CourierShopLoginApp.Helpers
{
    /// <summary>
    /// Contains global application configuration settings and constants
    /// </summary>
    public static class GlobalConfig
    {
        private static bool _isInitialized = false;
        public static string ConnectionString { get; private set; }
        
        public static void InitializeConfig()
        {
            // Проверяем, был ли уже инициализирован конфиг
            if (_isInitialized)
            {
                return;
            }
            
            try
            {
                // Hardcode connection string directly from app.config
                ConnectionString = "Data Source=t1brime-dev.ru;Initial Catalog=CourierShopDB;User ID=Apelsin;Password=gHunjkimjhngbtfder4";
                
                // If connection string is not set, throw an exception
                if (string.IsNullOrEmpty(ConnectionString))
                    throw new InvalidOperationException("Connection string not initialized.");
                
                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("GlobalConfig initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GlobalConfig.InitializeConfig: {ex.Message}");
                throw;
            }
        }
        
        // Добавляем метод для проверки инициализации
        public static bool IsInitialized()
        {
            return _isInitialized && !string.IsNullOrEmpty(ConnectionString);
        }
    }
}
