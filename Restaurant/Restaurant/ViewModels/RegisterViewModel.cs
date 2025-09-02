using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Services;
using Restaurant.Views;

namespace Restaurant.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private readonly RestaurantDbContext _db;
        private readonly NavigationService _nav;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string DeliveryAddress { get; set; }   
        public string Password { get; set; }

        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        public RegisterViewModel(
            RestaurantDbContext db,
            NavigationService nav)
        {
            _db = db;
            _nav = nav;

            RegisterCommand = new RelayCommand(async _ => await OnRegisterAsync());
            CancelCommand = new RelayCommand(_ => _nav.Navigate<LoginPage>()); 
        }

        private async System.Threading.Tasks.Task OnRegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Completează toate câmpurile obligatorii.", "Atenție", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var exists = await _db.Users.AnyAsync(u => u.Email == Email);
            if (exists)
            {
                MessageBox.Show("Există deja un cont cu acest email.", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var user = new User
            {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Phone = Phone,
                DeliveryAddress = DeliveryAddress,
                PasswordHash = Hash(Password),
                Role = "Client"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            MessageBox.Show("Cont creat cu succes!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

            _nav.Navigate<LoginPage>(); 
        }

        private byte[] Hash(string password)
            => System.Text.Encoding.UTF8.GetBytes(password);
    }
}
