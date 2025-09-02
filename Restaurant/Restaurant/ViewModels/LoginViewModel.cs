using System;
using System.ComponentModel;
using System.Linq;                        
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Services;
using Restaurant.Views;          

namespace Restaurant.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly SessionService _session;
        private readonly NavigationService _nav;
        private readonly RestaurantDbContext _db;

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

     
        public string Email { get; set; }
        public string Password { get; set; }

      
        private UserType _userType = UserType.Client;
        public bool IsClientMode
        {
            get => _userType == UserType.Client;
            set { if (value) ChangeMode(UserType.Client); }
        }
        public bool IsEmployeeMode
        {
            get => _userType == UserType.Employee;
            set { if (value) ChangeMode(UserType.Employee); }
        }
        private void ChangeMode(UserType mode)
        {
            _userType = mode;
            OnPropertyChanged(nameof(IsClientMode));
            OnPropertyChanged(nameof(IsEmployeeMode));
        }
        private enum UserType { Client, Employee }


        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand GuestCommand { get; }

        public LoginViewModel(
            SessionService session,
            NavigationService nav,
            RestaurantDbContext db)
        {
            _session = session;
            _nav = nav;
            _db = db;

            LoginCommand = new RelayCommand(async _ => await DoLoginAsync());
            RegisterCommand = new RelayCommand(_ => _nav.Navigate<RegisterPage>());
            GuestCommand = new RelayCommand(_ => _nav.Navigate<GuestHomePage>());
        }

        private async Task DoLoginAsync()
        {
            var inputHash = Hash(Password);  

            var user = await _db.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == Email
                    && u.PasswordHash.SequenceEqual(inputHash)
                );

            if (user == null)
            {
                MessageBox.Show("Date invalide.", "Eroare", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsEmployeeMode && user.Role != "Employee")
            {
                MessageBox.Show("Nu eşti angajat.", "Acces refuzat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (IsClientMode && user.Role != "Client")
            {
                MessageBox.Show("Nu eşti client.", "Acces refuzat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _session.SignIn(user);

            if (IsEmployeeMode)
                _nav.Navigate<AdminPage>();
            else if (IsClientMode)
                _nav.Navigate<ClientHomePage>();
            else
                _nav.Navigate<ClientHomePage>();
        }

        private byte[] Hash(string plain)
            => System.Text.Encoding.UTF8.GetBytes(plain); 
    }
}
