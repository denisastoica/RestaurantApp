using System;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.Views;
using Restaurant.ViewModels;
using Restaurant.Data;

namespace Restaurant.Views
{
    public partial class ClientHomePage : Page
    {
        public ClientHomePage(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            MenuFrame.Navigate(serviceProvider.GetRequiredService<MenuPage>());

            // Modificare aici:
            var db = serviceProvider.GetRequiredService<RestaurantDbContext>();
            var cautareVM = new CautareViewModel(db);
            var cautareView = new CautareView { DataContext = cautareVM };
            SearchFrame.Navigate(cautareView);

            CartFrame.Navigate(serviceProvider.GetRequiredService<CartPage>());
            OrdersFrame.Navigate(serviceProvider.GetRequiredService<OrdersPage>());
        }
    }
}
