using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Restaurant.Models;
using Restaurant.Services;

namespace Restaurant.ViewModels
{
    public class OrderDisplayItem : INotifyPropertyChanged
    {
        public int OrderId { get; set; }
        public Guid Code { get; set; }
        public DateTime Date { get; set; }
        public DateTime? DeliveryEta { get; set; } 
        private OrderStatus _status;
        public OrderStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }
        public string StatusText => Status?.Status ?? "necunoscut";

        public string Items { get; set; }
        public decimal Total { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class OrdersViewModel : INotifyPropertyChanged
    {
        private readonly OrderService _orderService;
        private readonly SessionService _sessionService;

        public ObservableCollection<OrderDisplayItem> Orders { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand CancelCommand { get; }

        public OrdersViewModel(OrderService orderService, SessionService sessionService)
        {
            _orderService = orderService;
            _sessionService = sessionService;

            RefreshCommand = new RelayCommand(async _ => await LoadOrdersAsync());
            CancelCommand = new RelayCommand(async orderId => await CancelOrderAsync((int)orderId));

            _ = LoadOrdersAsync();
        }

        private async Task LoadOrdersAsync()
        {
            Orders.Clear();
            int currentUserId = GetCurrentUserId();

            var orders = await _orderService.GetOrdersByUserStoredProcAsync(currentUserId);

            foreach (var order in orders)
            {
                var productDescriptions = order.OrderItems.Select(i =>
                    $"{i.Quantity}*{i.Product?.Name ?? "[Produs lipsă]"}");

                var menuDescriptions = order.OrderMenuItems.Select(m =>
                    $"{m.Quantity}*{m.Menu?.Name ?? "[Meniu lipsă]"}");

                var itemsText = string.Join(", ", productDescriptions.Concat(menuDescriptions));

                Orders.Add(new OrderDisplayItem
                {
                    OrderId = order.OrderId,
                    Code = order.OrderCode,
                    Date = order.OrderDate, 
                    DeliveryEta = order.DeliveryEta,
                    Status = order.Status,
                    Items = itemsText,
                    Total = CalculateOrderTotal(order)
                });
            }
        }

        private decimal CalculateOrderTotal(Order order)
        {
            decimal total = 0m;
            total += order.OrderItems.Sum(i => i.UnitPrice * i.Quantity);
            total += order.OrderMenuItems.Sum(m => m.UnitPrice * m.Quantity);
            total -= order.Discount;
            total += order.DeliveryFee;
            return total;
        }

        private async Task CancelOrderAsync(int orderId)
        {
            await _orderService.UpdateOrderStatusAsync(orderId, "anulata");
            await LoadOrdersAsync();
        }

        private int GetCurrentUserId()
        {
            return _sessionService.CurrentUser?.UserId ?? 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
