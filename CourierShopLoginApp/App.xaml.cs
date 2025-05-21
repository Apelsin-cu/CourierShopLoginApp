using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CourierShopLoginApp.Helpers;
using System.Configuration;

namespace CourierShopLoginApp
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            try
            {
                // Initialize global configuration with hardcoded connection string
                GlobalConfig.InitializeConfig();
                
                // Check and initialize database structure
                bool isDatabaseReady = await DatabaseInitializer.EnsureDatabaseStructureAsync();
                if (!isDatabaseReady)
                {
                    MessageBox.Show("Не удалось инициализировать базу данных. Приложение может работать некорректно.", 
                        "Ошибка инициализации", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                
                // Log to console or debug output to verify connection string is set
                Console.WriteLine($"Connection string initialized: {!string.IsNullOrEmpty(GlobalConfig.ConnectionString)}");
                // OR System.Diagnostics.Debug.WriteLine($"Connection string initialized: {!string.IsNullOrEmpty(GlobalConfig.ConnectionString)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при запуске приложения: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
