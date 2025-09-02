using System;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.ViewModels;

namespace Restaurant.Views
{
    public partial class GuestHomePage : Page
    {
        public GuestHomePage(IServiceProvider provider)
        {
            InitializeComponent();

            // Ia ViewModel-ul
            var cautareVM = provider.GetRequiredService<CautareViewModel>();
            // Creează pagina și setează DataContext
            var cautareView = new CautareView { DataContext = cautareVM };

            // Similar și pentru MenuPage, dacă ai nevoie
            var menuPage = provider.GetRequiredService<MenuPage>();

            // Navighează spre pagini
            MenuFrame.Navigate(menuPage);
            SearchFrame.Navigate(cautareView);
        }
    }
}
