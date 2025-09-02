using Restaurant.ViewModels;
using System.Windows.Controls;

namespace Restaurant.Views
{
    public partial class LoginPage : Page
    {
        public LoginPage(LoginViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }

}
