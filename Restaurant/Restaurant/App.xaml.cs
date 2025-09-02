using Microsoft.Extensions.DependencyInjection;
using Restaurant.Data;
using Restaurant.Services;
using Restaurant.ViewModels;
using Restaurant.Views;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;

namespace Restaurant
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; }

        public App()
        {
            var services = new ServiceCollection();

            services.AddDbContext<RestaurantDbContext>(opts =>
                opts.UseSqlServer("…conexiunea ta…"));

            services.AddSingleton<SessionService>();
            services.AddSingleton<NavigationService>();
            services.AddSingleton<ConfigurationService>();
            services.AddSingleton<CartViewModel>();

            services.AddTransient<ClientHomePage>(sp =>
            {
                return new ClientHomePage(sp);
            });


            services.AddTransient<LoginViewModel>();
            services.AddTransient<MenuViewModel>();
            services.AddTransient<OrdersViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<OrderService>();
            services.AddTransient<AdminPage>();
            services.AddTransient<AdminViewModel>();
            services.AddTransient<CautareViewModel>();
            services.AddTransient<GuestHomePage>();
            services.AddTransient<GuestHomePageViewModel>();
            services.AddTransient<MenuPage>();
            services.AddTransient<CautareView>();

            services.AddTransient<LoginWindow>(); 
            services.AddTransient<LoginPage>();
            services.AddTransient<MenuPage>();
            services.AddTransient<CartPage>();
            services.AddTransient<OrdersPage>();
            services.AddTransient<RegisterPage>();
            Services = services.BuildServiceProvider();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configService = Services.GetRequiredService<ConfigurationService>();
            await configService.LoadConfigurationsAsync();

            var loginWindow = Services.GetRequiredService<LoginWindow>();
            loginWindow.Show();
        }


        private void OnDispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"Unhandled UI exception:\n\n{e.Exception.GetType()}\n{e.Exception.Message}\n\n{e.Exception.StackTrace}",
                "Fatal error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender,
            UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show(
                    $"Unhandled non-UI exception:\n\n{ex.GetType()}\n{ex.Message}\n\n{ex.StackTrace}",
                    "Fatal error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
