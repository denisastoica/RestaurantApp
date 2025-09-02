using System;
using System.Collections;
using System.Collections.Generic;
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
    public class AdminViewModel : INotifyPropertyChanged
    {
        private readonly RestaurantDbContext _db;
        public RestaurantDbContext DbContext => _db;                

        public DbContextOptions<RestaurantDbContext> DbOptions { get; }
        public ConfigurationService ConfigService { get; }

        private int _adminTabIndex;
        public int AdminTabIndex
        {
            get => _adminTabIndex;
            set { _adminTabIndex = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Category> Categories { get; } = new();
        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); SetCategoryFields(value); }
        }
        private string _categoryName;
        public string CategoryName
        {
            get => _categoryName;
            set { _categoryName = value; OnPropertyChanged(); }
        }
        public ICommand AddCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        public ObservableCollection<Product> Products { get; } = new();
        public ObservableCollection<Allergen> SelectedProductAllergens { get; } = new();

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); SetProductFields(value); }
        }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string ProductPortion { get; set; }
        public decimal ProductTotalQuantity { get; set; }
        public Category SelectedProductCategory { get; set; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }

        public ObservableCollection<Menu> Menus { get; } = new();
        private Menu _selectedMenu;
        public Menu SelectedMenu
        {
            get => _selectedMenu;
            set
            {
                if (_selectedMenu != value)
                {
                    _selectedMenu = value;
                    OnPropertyChanged();
                    SetMenuFields(_selectedMenu);
                }
            }
        }
        public string MenuName { get; set; }
        public decimal MenuDiscountPercent { get; set; }
        public Category SelectedMenuCategory { get; set; }
        public ICommand AddMenuCommand { get; }
        public ICommand EditMenuCommand { get; }
        public ICommand DeleteMenuCommand { get; }

        public ObservableCollection<Product> AllProducts { get; } = new();
        public IList SelectedMenuProducts { get; set; } = new ObservableCollection<Product>();
        public ObservableCollection<Product> SelectedMenuContents { get; } = new();

        public ObservableCollection<Allergen> Allergens { get; } = new();
        private Allergen _selectedAllergen;
        public Allergen SelectedAllergen
        {
            get => _selectedAllergen;
            set { _selectedAllergen = value; OnPropertyChanged(); SetAllergenFields(value); }
        }
        private string _allergenName;
        public string AllergenName
        {
            get => _allergenName;
            set { _allergenName = value; OnPropertyChanged(); }
        }
        public ICommand AddAllergenCommand { get; }
        public ICommand EditAllergenCommand { get; }
        public ICommand DeleteAllergenCommand { get; }

        public ObservableCollection<Product> SelectedAllergenProducts { get; } = new();
        private Product _selectedProductForAllergen;
        public Product SelectedProductForAllergen
        {
            get => _selectedProductForAllergen;
            set { _selectedProductForAllergen = value; OnPropertyChanged(); }
        }
        public ICommand EditAllergenProductCommand { get; }

        public AdminViewModel(
            RestaurantDbContext db,
            DbContextOptions<RestaurantDbContext> dbOptions,
            ConfigurationService configService)
        {
            _db = db;
            DbOptions = dbOptions;
            ConfigService = configService;

            AddCategoryCommand = new RelayCommand(async _ => await AddCategoryAsync());
            EditCategoryCommand = new RelayCommand(async _ => await EditCategoryAsync());
            DeleteCategoryCommand = new RelayCommand(async _ => await DeleteCategoryAsync());

            AddProductCommand = new RelayCommand(async _ => await AddProductAsync());
            EditProductCommand = new RelayCommand(async _ => await EditProductAsync());
            DeleteProductCommand = new RelayCommand(async _ => await DeleteProductAsync());

            AddMenuCommand = new RelayCommand(async _ => await AddMenuAsync());
            EditMenuCommand = new RelayCommand(async _ => await EditMenuAsync());
            DeleteMenuCommand = new RelayCommand<Menu>(
                async menu => await DeleteMenuAsync(menu),
                menu => menu != null
            );
            AddAllergenCommand = new RelayCommand(async _ => await AddAllergenAsync());
            EditAllergenCommand = new RelayCommand(async _ => await EditAllergenAsync());
            DeleteAllergenCommand = new RelayCommand(async _ => await DeleteAllergenAsync());

            EditAllergenProductCommand = new RelayCommand<Product>(EditAllergenProduct, p => p != null);

            InitializeAllData();
        }

        private async void InitializeAllData()
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
            await LoadMenusAsync();
            await LoadAllProductsAsync();
            await LoadAllergensAsync();
        }

        public async Task LoadCategoriesAsync()
        {
            Categories.Clear();
            var cats = await _db.Categories.FromSqlRaw("EXEC sp_GetAllCategories").ToListAsync();
            foreach (var c in cats)
                Categories.Add(c);
        }

        public async Task LoadProductsAsync()
        {
            Products.Clear();
            using var db = new RestaurantDbContext(DbOptions);
            var list = await db.Products
                               .Include(p => p.Category)
                               .Include(p => p.Allergens)       
                               .ToListAsync();
            foreach (var p in list)
                Products.Add(p);
        }

        public async Task LoadAllProductsAsync()
        {
            AllProducts.Clear();
            using var db = new RestaurantDbContext(DbOptions);
            var produse = await db.Products.ToListAsync();
            foreach (var p in produse)
                AllProducts.Add(p);
        }

        public async Task LoadMenusAsync()
        {
            Menus.Clear();

            var menus = await _db.Menus
                                .Include(m => m.Category)
                                    .Include(m => m.MenuPhotos)                       
                                .Include(m => m.MenuItems)
                                    .ThenInclude(mi => mi.Product)
                                .ToListAsync();

            foreach (var m in menus)
                Menus.Add(m);
        }


        public async Task LoadAllergensAsync()
        {
            Allergens.Clear();
            using var db = new RestaurantDbContext(DbOptions);
            var alergeni = await db.Allergens.ToListAsync();
            foreach (var a in alergeni)
                Allergens.Add(a);
        }

        private async Task AddCategoryAsync()
        {
            var newCat = new Category { Name = CategoryName ?? "Noua categorie" };
            _db.Categories.Add(newCat);
            await _db.SaveChangesAsync();
            await LoadCategoriesAsync();
            SetCategoryFields(null);
        }

        private async Task EditCategoryAsync()
        {
            if (SelectedCategory == null) return;
            SelectedCategory.Name = CategoryName;
            await _db.SaveChangesAsync();
            await LoadCategoriesAsync();
        }

        private async Task DeleteCategoryAsync()
        {
            if (SelectedCategory == null) return;
            await _db.SaveChangesAsync();
            await LoadCategoriesAsync();
            SetCategoryFields(null);
            SelectedCategory = null;
        }

        private void SetCategoryFields(Category c)
            => CategoryName = c?.Name ?? "";

       private async Task AddProductAsync()
    {
        var newProd = new Product {
            Name          = ProductName,
            Price         = ProductPrice,
            PortionSize   = ProductPortion,
            TotalQuantity = ProductTotalQuantity,
            CategoryId    = SelectedProductCategory.CategoryId,
        };
        _db.Products.Add(newProd);
        await _db.SaveChangesAsync();

        await LoadProductsAsync();
        SetProductFields(null);
    }


        private async Task EditProductAsync()
        {
            if (SelectedProduct == null) return;

            SelectedProduct.Name = ProductName;
            SelectedProduct.Price = ProductPrice;
            SelectedProduct.PortionSize = ProductPortion;
            SelectedProduct.TotalQuantity = ProductTotalQuantity;
            SelectedProduct.CategoryId = SelectedProductCategory.CategoryId;

            SelectedProduct.Allergens.Clear();
            foreach (var al in SelectedProductAllergens)
                SelectedProduct.Allergens.Add(al);

            await _db.SaveChangesAsync();

            await LoadProductsAsync();
            SetProductFields(SelectedProduct);
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null) return;
            await _db.SaveChangesAsync();
            await LoadProductsAsync();
            await LoadAllProductsAsync();
            SetProductFields(null);
            SelectedProduct = null;
        }

        private void SetProductFields(Product p)
        {
            if (p == null)
            {
                ProductName = "";
                ProductPrice = 0;
                ProductPortion = "";
                ProductTotalQuantity = 0;
                SelectedProductCategory = null;
                SelectedProductAllergens.Clear();
            }
            else
            {
                ProductName = p.Name;
                ProductPrice = p.Price;
                ProductPortion = p.PortionSize;
                ProductTotalQuantity = p.TotalQuantity;
                SelectedProductCategory = Categories.First(c => c.CategoryId == p.CategoryId);

                SelectedProductAllergens.Clear();
                foreach (var al in p.Allergens)
                    SelectedProductAllergens.Add(al);
            }

            OnPropertyChanged(nameof(ProductName));
            OnPropertyChanged(nameof(ProductPrice));
            OnPropertyChanged(nameof(ProductPortion));
            OnPropertyChanged(nameof(ProductTotalQuantity));
            OnPropertyChanged(nameof(SelectedProductCategory));
            OnPropertyChanged(nameof(SelectedProductAllergens));
        }
        private async Task AddMenuAsync()
        {
            var newMenu = new Menu
            {
                Name = MenuName ?? "Noul meniu",
                DiscountPercent = MenuDiscountPercent,
                CategoryId = SelectedMenuCategory.CategoryId
            };
            _db.Menus.Add(newMenu);
            await _db.SaveChangesAsync();
            foreach (Product p in SelectedMenuProducts.Cast<Product>())
                _db.MenuItems.Add(new MenuItem { MenuId = newMenu.MenuId, ProductId = p.ProductId, QuantityInMenu = 1 });
            await _db.SaveChangesAsync();
            await LoadMenusAsync();
            SetMenuFields(null);
        }

        private async Task EditMenuAsync()
        {
            if (SelectedMenu == null) return;
            SelectedMenu.Name = MenuName;
            SelectedMenu.DiscountPercent = MenuDiscountPercent;
            SelectedMenu.CategoryId = SelectedMenuCategory.CategoryId;
            await _db.SaveChangesAsync();
            var old = _db.MenuItems.Where(mi => mi.MenuId == SelectedMenu.MenuId);
            _db.MenuItems.RemoveRange(old);
            foreach (Product p in SelectedMenuProducts.Cast<Product>())
                _db.MenuItems.Add(new MenuItem { MenuId = SelectedMenu.MenuId, ProductId = p.ProductId, QuantityInMenu = 1 });
            await _db.SaveChangesAsync();
            await LoadMenusAsync();
        }

        private async Task DeleteMenuAsync(Menu menu)
        {
            if (menu == null) return;

            bool isUsed = await _db.OrderMenuItems
                                   .AnyAsync(omi => omi.MenuId == menu.MenuId);
            if (isUsed)
            {
                System.Windows.MessageBox.Show(
                    "Nu poți șterge acest meniu pentru că există comenzi care îl conțin.",
                    "Ștergere imposibilă",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            await _db.Entry(menu)
                     .Collection(m => m.MenuItems)
                     .LoadAsync();

            _db.MenuItems.RemoveRange(menu.MenuItems);

            var photos = _db.Set<MenuPhoto>()
                            .Where(mp => mp.MenuId == menu.MenuId);
            _db.Set<MenuPhoto>().RemoveRange(photos);

            _db.Menus.Remove(menu);

            await _db.SaveChangesAsync();

            await LoadMenusAsync();
            SelectedMenu = null;
            SetMenuFields(null);
        }



        private void SetMenuFields(Menu m)
        {
            if (m == null)
            {
                MenuName = "";
                MenuDiscountPercent = 0;
                SelectedMenuCategory = null;
                SelectedMenuProducts.Clear();
                SelectedMenuContents.Clear();
            }
            else
            {
                MenuName = m.Name;
                MenuDiscountPercent = m.DiscountPercent;
                SelectedMenuCategory = Categories.First(c => c.CategoryId == m.CategoryId);

                SelectedMenuProducts.Clear();
                foreach (var mi in m.MenuItems)
                    SelectedMenuProducts.Add(mi.Product);

                SelectedMenuContents.Clear();
                foreach (var mi in m.MenuItems)
                    SelectedMenuContents.Add(mi.Product);
            }

            OnPropertyChanged(nameof(MenuName));
            OnPropertyChanged(nameof(MenuDiscountPercent));
            OnPropertyChanged(nameof(SelectedMenuCategory));
            OnPropertyChanged(nameof(SelectedMenuProducts));
            OnPropertyChanged(nameof(SelectedMenuContents));
        }

        private async Task AddAllergenAsync()
        {
            var newAllergen = new Allergen { Name = AllergenName ?? "Noul alergen" };
            _db.Allergens.Add(newAllergen);
            await _db.SaveChangesAsync();
            await LoadAllergensAsync();
            SetAllergenFields(null);
        }

        private async Task EditAllergenAsync()
        {
            if (SelectedAllergen == null) return;
            SelectedAllergen.Name = AllergenName;
            await _db.SaveChangesAsync();
            await LoadAllergensAsync();
        }

        private async Task DeleteAllergenAsync()
        {
            if (SelectedAllergen == null) return;
            _db.Allergens.Remove(SelectedAllergen);
            await _db.SaveChangesAsync();
            await LoadAllergensAsync();
            SetAllergenFields(null);
        }

        private void SetAllergenFields(Allergen a)
        {
            AllergenName = a?.Name ?? "";
            SelectedAllergenProducts.Clear();
            if (a != null)
            {
                var produse = _db.Products
                                 .Include(p => p.Allergens)
                                 .Where(p => p.Allergens.Any(al => al.AllergenId == a.AllergenId))
                                 .ToList();
                foreach (var p in produse)
                    SelectedAllergenProducts.Add(p);
            }
            OnPropertyChanged(nameof(AllergenName));
            OnPropertyChanged(nameof(SelectedAllergenProducts));
        }

        private void EditAllergenProduct(Product prod)
        {
            if (prod == null) return;
            AdminTabIndex = 1;           
            SelectedProduct = prod;     
            SetProductFields(prod);    
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
