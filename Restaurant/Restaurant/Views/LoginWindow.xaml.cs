using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Restaurant.Services;
using Restaurant.ViewModels;
using Restaurant.Views;

namespace Restaurant.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var nav = serviceProvider.GetRequiredService<NavigationService>();

            // Asigură-te că în XAML există Frame-ul MainFrame cu x:Name="MainFrame"
            nav.Frame = MainFrame;

            // Navighează la LoginPage în Frame
            nav.Navigate<LoginPage>();

            DataContext = serviceProvider.GetRequiredService<LoginViewModel>();
        }
    }
}
