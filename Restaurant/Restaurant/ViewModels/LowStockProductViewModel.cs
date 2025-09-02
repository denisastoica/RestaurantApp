using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Services;

namespace Restaurant.ViewModels
{
    public class LowStockProductsViewModel : INotifyPropertyChanged
    {
        private readonly RestaurantDbContext _db;
        private readonly ConfigurationService _config;

        public ObservableCollection<Product> LowStockProducts { get; } = new();

        private decimal _threshold;
        public decimal Threshold
        {
            get => _threshold;
            private set { _threshold = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }

        public LowStockProductsViewModel(RestaurantDbContext db, ConfigurationService config)
        {
            _db = db;
            _config = config;
            RefreshCommand = new RelayCommand(async _ => await LoadProductsAsync());
            _ = LoadProductsAsync();
        }

        public async Task LoadProductsAsync()
        {
            Threshold = _config.GetDecimal("LowStockThreshold", 5m);

            LowStockProducts.Clear();

            var results = await _db.Set<LowStockWithStatsDto>()
                .FromSqlRaw("EXEC dbo.sp_GetLowStockWithStats @p0", Threshold)
                .ToListAsync();

            foreach (var item in results)
            {
                LowStockProducts.Add(new Product
                {
                    ProductId = item.ProductId,
                    Name = item.ProductName,
                    TotalQuantity = item.TotalQuantity,
                    Category = new Category { Name = item.CategoryName }
                });
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

}
