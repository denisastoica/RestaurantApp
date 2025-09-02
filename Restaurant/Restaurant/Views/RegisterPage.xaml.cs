using System.Windows.Controls;
using Restaurant.ViewModels;

namespace Restaurant.Views
{
    public partial class RegisterPage : Page
    {
        private readonly RegisterViewModel _vm;

        public RegisterPage(RegisterViewModel vm)
        {
            InitializeComponent();
            DataContext = _vm = vm;

            // actualizăm parola în ViewModel
            PwdBox.PasswordChanged += (s, e) =>
            {
                _vm.Password = PwdBox.Password;
            };
        }
    }
}
