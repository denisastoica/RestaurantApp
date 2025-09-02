using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Restaurant.Models;
using Restaurant.Services;
using Restaurant.Views;

namespace Restaurant.ViewModels
{
    public class CartItemViewModel : INotifyPropertyChanged
    {
        public int? ProductId { get; }
        public int? MenuId { get; }
        public string Name { get; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Subtotal));
                    QuantityChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public decimal UnitPrice { get; }
        public decimal Subtotal => UnitPrice * Quantity;

        public CartItemViewModel(Product product, int quantity)
        {
            ProductId = product.ProductId;
            Name = product.Name;
            _quantity = quantity;
            UnitPrice = product.Price;
        }

        public CartItemViewModel(Restaurant.Models.Menu menu, int quantity, decimal unitPrice)
        {
            MenuId = menu.MenuId;
            Name = menu.Name;
            _quantity = quantity;
            UnitPrice = unitPrice;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler QuantityChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class CartViewModel : INotifyPropertyChanged
    {
        private readonly OrderService _orderService;
        private readonly NavigationService _navigationService;
        private readonly ConfigurationService _config;
        private readonly SessionService _session;   // <-- INJECTAT

        public ObservableCollection<CartItemViewModel> Items { get; } = new();

        private decimal _discount;
        public decimal Discount
        {
            get => _discount;
            set { _discount = value; OnPropertyChanged(); OnPropertyChanged(nameof(Total)); }
        }

        private decimal _deliveryFee;
        public decimal DeliveryFee
        {
            get => _deliveryFee;
            set { _deliveryFee = value; OnPropertyChanged(); OnPropertyChanged(nameof(Total)); }
        }

        public decimal Subtotal => Items.Sum(i => i.Subtotal);
        public decimal Total => Subtotal - Discount + DeliveryFee;

        public ICommand BackCommand { get; }
        public ICommand PlaceOrderCommand { get; }

        public CartViewModel(OrderService orderService, NavigationService navigationService, ConfigurationService config, SessionService session)
        {
            _orderService = orderService;
            _navigationService = navigationService;
            _config = config;
            _session = session;

            BackCommand = new RelayCommand(_ => _navigationService.GoBack());
            PlaceOrderCommand = new RelayCommand(async _ => await PlaceOrderAsync());

            Items.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (CartItemViewModel item in e.NewItems)
                        item.QuantityChanged += (sender, _) => RecalculateDiscountAndDelivery();
                }
                RecalculateDiscountAndDelivery();
                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(Total));
            };
        }

        public void AddProduct(Product product, int quantity)
        {
            if (product == null || quantity <= 0) return;

            var existing = Items.FirstOrDefault(i => i.ProductId == product.ProductId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                var newItem = new CartItemViewModel(product, quantity);
                newItem.QuantityChanged += (sender, _) => RecalculateDiscountAndDelivery();
                Items.Add(newItem);
            }
            RecalculateDiscountAndDelivery();
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
        }

        public void AddMenu(Restaurant.Models.Menu menu, int quantity, decimal price)
        {
            if (menu == null || quantity <= 0) return;

            var existing = Items.FirstOrDefault(i => i.MenuId == menu.MenuId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                var newItem = new CartItemViewModel(menu, quantity, price);
                newItem.QuantityChanged += (sender, _) => RecalculateDiscountAndDelivery();
                Items.Add(newItem);
            }
            RecalculateDiscountAndDelivery();
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
        }

        private void RecalculateDiscountAndDelivery()
        {
            decimal minSumForDiscount = _config.GetDecimal("DiscountThreshold", 150m);
            decimal discountPercent = _config.GetDecimal("DiscountPercent", 15m);
            decimal minSumForDeliveryFee = _config.GetDecimal("DeliveryFeeBelowAmount", 50m);
            decimal deliveryFeeAmount = _config.GetDecimal("DeliveryFee", 7m);

            decimal subtotal = Subtotal;

            decimal discount = subtotal >= minSumForDiscount ? subtotal * discountPercent / 100m : 0m;
            decimal deliveryFee = subtotal < minSumForDeliveryFee ? deliveryFeeAmount : 0m;

            Discount = discount;
            DeliveryFee = deliveryFee;
            OnPropertyChanged(nameof(Discount));
            OnPropertyChanged(nameof(DeliveryFee));
            OnPropertyChanged(nameof(Total));
        }

        private async Task PlaceOrderAsync()
        {
            var productOrders = Items
                .Where(i => i.ProductId.HasValue)
                .Select(i => (ProductId: i.ProductId.Value, Quantity: i.Quantity, UnitPrice: i.UnitPrice))
                .ToList();

            var menuOrders = Items
                .Where(i => i.MenuId.HasValue)
                .Select(i => (MenuId: i.MenuId.Value, Quantity: i.Quantity, UnitPrice: i.UnitPrice))
                .ToList();

            int userId = GetCurrentUserId();
            if (userId == 0)
            {
                System.Windows.MessageBox.Show("Trebuie să fii autentificat pentru a plasa o comandă.", "Autentificare", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var order = await _orderService.CreateOrderAsync(
                userId: userId,
                products: productOrders,
                menus: menuOrders);


            Items.Clear();
            Discount = 0;
            DeliveryFee = 0;
            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
        }
        private int GetCurrentUserId()
            => _session.CurrentUser?.UserId ?? 0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
