using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Services;
using Restaurant.Views;

namespace Restaurant.ViewModels
{
    public class MenuViewModel : INotifyPropertyChanged
    {
        private readonly RestaurantDbContext _db;
        private readonly NavigationService _nav;
        private readonly ConfigurationService _configService;
        private readonly CartViewModel _cartViewModel;
        private readonly SemaphoreSlim _loadLock = new(1, 1);

        private decimal _menuDiscountPercent;

        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<object> Items { get; } = new();
        public ObservableCollection<Allergen> Allergens { get; } = new();

        public string SearchText { get; set; }
        public Allergen SelectedAllergen { get; set; }

        public ICommand SearchCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand GoToCartCommand { get; }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory == value) return;
                _selectedCategory = value;
                OnPropertyChanged();
                _ = LoadItemsAsync();
            }
        }


        public MenuViewModel(
    RestaurantDbContext db,
    NavigationService nav,
    ConfigurationService configService,
    CartViewModel cartViewModel)
        {
            _db = db;
            _nav = nav;
            _configService = configService;
            _cartViewModel = cartViewModel;

            _menuDiscountPercent = _configService.GetDecimal("MenuDiscountPercent", 0m);

            SearchCommand = new RelayCommand(async _ => await LoadItemsAsync());

            AddToCartCommand = new RelayCommand<object>(item =>
            {
                if (item == null) return;

                if (item is Product product)
                {
                    _cartViewModel.AddProduct(product, 1);
                }
                else if (item is MenuDisplayItem menuItem)
                {
                        _cartViewModel.AddMenu(menuItem.Menu, 1, menuItem.CalculatedPrice);
                }
            });


            GoToCartCommand = new RelayCommand(_ => _nav.Navigate<CartPage>());

            _ = LoadCategoriesAsync();
            _ = LoadAllergensAsync();
        }

        public async Task LoadCategoriesAsync()
        {
            Categories.Clear();
            var cats = await _db.Categories.ToListAsync();
            foreach (var c in cats)
                Categories.Add(c);

            if (Categories.Any())
                SelectedCategory = Categories[0];
        }



        public async Task LoadAllergensAsync()
        {
            await _loadLock.WaitAsync();
            try
            {
                Allergens.Clear();
                var alls = await _db.Allergens.ToListAsync();
                foreach (var a in alls) Allergens.Add(a);
            }
            finally
            {
                _loadLock.Release();
            }
        }

        public async Task LoadItemsAsync()
        {
            await _loadLock.WaitAsync();
            try
            {
                if (SelectedCategory == null) return;
                Items.Clear();

                var prodQ = _db.Products
                    .Include(p => p.ProductPhotos)
                    .Include(p => p.Allergens)
                    .Where(p => p.CategoryId == SelectedCategory.CategoryId);

                if (!string.IsNullOrWhiteSpace(SearchText))
                    prodQ = prodQ.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                if (SelectedAllergen != null)
                    prodQ = prodQ.Where(p => !p.Allergens.Any(a => a.AllergenId == SelectedAllergen.AllergenId));

                var prods = await prodQ.ToListAsync();
                foreach (var p in prods)
                {
                    Items.Add(p);
                }

                var menuQ = _db.Menus
     .Include(m => m.MenuItems)
         .ThenInclude(mi => mi.Product)
     .Include(m => m.MenuPhotos)
     .Where(m => m.CategoryId == SelectedCategory.CategoryId);

                if (!string.IsNullOrWhiteSpace(SearchText))
                    menuQ = menuQ.Where(m => m.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

                var menus = await menuQ.ToListAsync();
                foreach (var menu in menus)
                {
                    decimal priceWithDiscount = CalculateMenuPrice(menu);
                    Items.Add(new MenuDisplayItem(menu, priceWithDiscount));
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcarea elementelor:\n{ex.Message}",
                                "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _loadLock.Release();
            }
        }

        public decimal CalculateMenuPrice(Menu menu)
        {
            if (menu.MenuItems == null || !menu.MenuItems.Any())
                return 0m;

            decimal total = 0m;

            foreach (var mi in menu.MenuItems)
            {
                decimal portionSizeGrams = ExtractGrams(mi.Product.PortionSize);

                decimal itemTotal = 0m;
                if (portionSizeGrams > 0)
                    itemTotal = (mi.QuantityInMenu / portionSizeGrams) * mi.Product.Price;
                else
                    itemTotal = mi.QuantityInMenu * mi.Product.Price;

                total += itemTotal;
            }

            decimal discountAmount = total * _menuDiscountPercent / 100m;
            return total - discountAmount;
        }

        private decimal ExtractGrams(string portionSize)
        {
            if (string.IsNullOrEmpty(portionSize))
                return 0m;

            var digits = new string(portionSize.TakeWhile(c => char.IsDigit(c)).ToArray());

            if (decimal.TryParse(digits, out var grams))
                return grams;

            return 0m;
        }
       
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

}
