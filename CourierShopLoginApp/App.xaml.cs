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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize global configuration with hardcoded connection string
            GlobalConfig.InitializeConfig();
            
            // Log to console or debug output to verify connection string is set
            Console.WriteLine($"Connection string initialized: {!string.IsNullOrEmpty(GlobalConfig.ConnectionString)}");
            // OR System.Diagnostics.Debug.WriteLine($"Connection string initialized: {!string.IsNullOrEmpty(GlobalConfig.ConnectionString)}");
        }
    }
}
