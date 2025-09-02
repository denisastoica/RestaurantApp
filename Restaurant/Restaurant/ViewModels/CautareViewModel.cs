using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.Data;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Restaurant.Services;

namespace Restaurant.ViewModels
{
    public class SearchResultItem
    {
        public string CategoryName { get; set; }
        public bool IsMenu { get; set; }
        public string Name { get; set; }
        public string PhotoUrl { get; set; }
        public string PortionSize { get; set; }
        public decimal Price { get; set; }
        public string AllergensText { get; set; }
    }

    public class CautareViewModel : INotifyPropertyChanged
    {
        private readonly RestaurantDbContext _db;
        public ObservableCollection<SearchCategoryGroup> GroupedResults { get; } = new();

        private string _cuvantCheie;
        public string CuvantCheie
        {
            get => _cuvantCheie;
            set { _cuvantCheie = value; OnPropertyChanged(); }
        }

        private string _alergenCheie;
        public string AlergenCheie
        {
            get => _alergenCheie;
            set { _alergenCheie = value; OnPropertyChanged(); }
        }

        private bool _tipFiltruContine = true;
        public bool TipFiltruContine
        {
            get => _tipFiltruContine;
            set { _tipFiltruContine = value; OnPropertyChanged(); }
        }

        public ICommand SearchCommand { get; }

        public CautareViewModel(RestaurantDbContext db)
        {
            _db = db;
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
        }

        public async Task SearchAsync()
        {
            GroupedResults.Clear();

            var categories = await _db.Categories.ToListAsync();

            foreach (var category in categories)
            {
                var group = new SearchCategoryGroup { CategoryName = category.Name };

                var productsQ = _db.Products
                    .Include(p => p.ProductPhotos)
                    .Include(p => p.Allergens)
                    .Where(p => p.CategoryId == category.CategoryId);

                if (!string.IsNullOrWhiteSpace(CuvantCheie))
                {
                    if (TipFiltruContine)
                        productsQ = productsQ.Where(p => p.Name.ToLower().Contains(CuvantCheie.ToLower()));
                    else
                        productsQ = productsQ.Where(p => !p.Name.ToLower().Contains(CuvantCheie.ToLower()));
                }
                if (!string.IsNullOrWhiteSpace(AlergenCheie))
                {
                    if (TipFiltruContine)
                        productsQ = productsQ.Where(p => p.Allergens.Any(a => a.Name.ToLower().Contains(AlergenCheie.ToLower())));
                    else
                        productsQ = productsQ.Where(p => !p.Allergens.Any(a => a.Name.ToLower().Contains(AlergenCheie.ToLower())));
                }

                var products = await productsQ.ToListAsync();
                foreach (var p in products)
                {
                    group.Items.Add(new SearchResultItem
                    {
                        CategoryName = category.Name,
                        IsMenu = false,
                        Name = p.Name,
                        PhotoUrl = p.PhotoUrl,
                        PortionSize = p.PortionSize,
                        Price = p.Price,
                        AllergensText = p.AllergensText
                    });
                }

      
                var menusQ = _db.Menus
                    .Include(m => m.MenuItems).ThenInclude(mi => mi.Product).ThenInclude(pr => pr.Allergens)
                    .Include(m => m.MenuPhotos)
                    .Where(m => m.CategoryId == category.CategoryId);

               
                if (!string.IsNullOrWhiteSpace(CuvantCheie))
                {
                    if (TipFiltruContine)
                        menusQ = menusQ.Where(m =>
                            m.Name.ToLower().Contains(CuvantCheie.ToLower()) ||
                            m.MenuItems.Any(mi => mi.Product.Name.ToLower().Contains(CuvantCheie.ToLower())));
                    else
                        menusQ = menusQ.Where(m =>
                            !m.Name.ToLower().Contains(CuvantCheie.ToLower()) &&
                            m.MenuItems.All(mi => !mi.Product.Name.ToLower().Contains(CuvantCheie.ToLower())));
                }


                
                if (!string.IsNullOrWhiteSpace(AlergenCheie))
                {
                    if (TipFiltruContine)
                        menusQ = menusQ.Where(m =>
                            m.MenuItems.Any(mi => mi.Product.Allergens.Any(a => a.Name.ToLower().Contains(AlergenCheie.ToLower()))));
                    else
                        menusQ = menusQ.Where(m =>
                            m.MenuItems.All(mi => !mi.Product.Allergens.Any(a => a.Name.ToLower().Contains(AlergenCheie.ToLower()))));
                }

                var menus = await menusQ.ToListAsync();
                foreach (var m in menus)
                {
                    var allAlergens = m.MenuItems.SelectMany(mi => mi.Product.Allergens).Select(a => a.Name).Distinct();
                    decimal pret = 0;
                    if (m.MenuItems != null && m.MenuItems.Any())
                    {
                        foreach (var mi in m.MenuItems)
                        {
                            if (mi.Product != null)
                            {
                                decimal portionSizeGrams = ExtractGrams(mi.Product.PortionSize);
                                decimal itemTotal = portionSizeGrams > 0
                                    ? (mi.QuantityInMenu / portionSizeGrams) * mi.Product.Price
                                    : mi.QuantityInMenu * mi.Product.Price;
                                pret += itemTotal;
                            }
                        }
                    }
                    group.Items.Add(new SearchResultItem
                    {
                        CategoryName = category.Name,
                        IsMenu = true,
                        Name = m.Name,
                        PhotoUrl = m.MenuPhotos.FirstOrDefault()?.PhotoUrl ?? "images/default_menu.jpg",
                        PortionSize = string.Join(", ", m.MenuItems.Select(mi =>
                            mi.Product != null ? $"{mi.Product.Name} x{mi.QuantityInMenu}" : "")),
                        Price = pret,
                        AllergensText = allAlergens.Any() ? string.Join(", ", allAlergens) : "Fără alergeni"
                    });
                }

                if (group.Items.Any())
                    GroupedResults.Add(group);
            }
        }

        public class SearchCategoryGroup
        {
            public string CategoryName { get; set; }
            public ObservableCollection<SearchResultItem> Items { get; set; } = new();
        }

        private decimal ExtractGrams(string portionSize)
        {
            if (string.IsNullOrEmpty(portionSize))
                return 0m;

            var digits = new string(portionSize.TakeWhile(char.IsDigit).ToArray());
            if (decimal.TryParse(digits, out var grams))
                return grams;
            return 0m;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
