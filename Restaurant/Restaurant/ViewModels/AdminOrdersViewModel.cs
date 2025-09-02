using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.Data;
using Restaurant.Services;


namespace Restaurant.ViewModels
{
    public class AdminOrderDisplayItem : INotifyPropertyChanged
    {
        public int OrderId { get; set; }
        public Guid Code { get; set; }
        public DateTime Date { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string ClientAddress { get; set; }
        public string Items { get; set; }
        public decimal FoodCost { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalCost { get; set; }
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string prop = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class AdminOrdersViewModel : INotifyPropertyChanged
    {
        private readonly RestaurantDbContext _db;
        private readonly OrderService _orderService;

        public ObservableCollection<AdminOrderDisplayItem> Orders { get; } = new();

        private bool _showOnlyActive;
        public bool ShowOnlyActive
        {
            get => _showOnlyActive;
            set { _showOnlyActive = value; OnPropertyChanged(); _ = LoadOrdersAsync(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ChangeStatusCommand { get; }

        public AdminOrdersViewModel(RestaurantDbContext db, OrderService orderService)
        {
            _db = db;
            _orderService = orderService;

            RefreshCommand = new RelayCommand(async _ => await LoadOrdersAsync());
            ChangeStatusCommand = new RelayCommand(async o =>
            {
                if (o is AdminOrderDisplayItem item)
                    await ChangeOrderStatusAsync(item);
            });

            _ = LoadOrdersAsync();
        }

        public async Task LoadOrdersAsync(int? userId = null)
        {
            Orders.Clear();

            var rawList = await _db.Set<OrderWithDetailsDto>()
                .FromSqlRaw("EXEC sp_GetOrdersWithDetailsDenormalized @UserId = {0}", userId ?? (object)DBNull.Value)
                .ToListAsync();

            var groupedOrders = rawList.GroupBy(o => o.OrderId);

            foreach (var group in groupedOrders)
            {
                var first = group.First();

                var products = group
                    .Where(x => x.ProductId.HasValue)
                    .Select(x => new ProductDto
                    {
                        ProductId = x.ProductId.Value,
                        ProductName = x.ProductName ?? "",
                        Quantity = x.ProductQuantity ?? 0,
                        UnitPrice = x.ProductUnitPrice ?? 0
                    })
                    .GroupBy(p => p.ProductId)
                    .Select(g => new ProductDto
                    {
                        ProductId = g.Key,
                        ProductName = g.First().ProductName,
                        Quantity = g.Sum(p => p.Quantity),
                        UnitPrice = g.First().UnitPrice
                    }).ToList();

                var menus = group
                    .Where(x => x.MenuId.HasValue)
                    .Select(x => new MenuDto
                    {
                        MenuId = x.MenuId.Value,
                        MenuName = x.MenuName ?? "",
                        Quantity = x.MenuQuantity ?? 0,
                        UnitPrice = x.MenuUnitPrice ?? 0
                    })
                    .GroupBy(m => m.MenuId)
                    .Select(g => new MenuDto
                    {
                        MenuId = g.Key,
                        MenuName = g.First().MenuName,
                        Quantity = g.Sum(m => m.Quantity),
                        UnitPrice = g.First().UnitPrice
                    }).ToList();

                string statusStr = first.Status?.Trim().ToLower() ?? "";

                if (ShowOnlyActive && (statusStr == "livrata" || statusStr == "anulata"))
                    continue;

                var productDescriptions = products.Select(i => $"{i.Quantity} x {i.ProductName}");
                var menuDescriptions = menus.Select(m => $"{m.Quantity} x {m.MenuName}");

                var itemsText = string.Join(", ", productDescriptions.Concat(menuDescriptions));

                Orders.Add(new AdminOrderDisplayItem
                {
                    OrderId = first.OrderId,
                    Code = first.OrderCode,
                    Date = first.OrderDate,
                    ClientName = $"{first.FirstName} {first.LastName}",
                    ClientPhone = first.Phone,
                    ClientAddress = first.DeliveryAddress,
                    Items = itemsText,
                    FoodCost = products.Sum(i => i.UnitPrice * i.Quantity) + menus.Sum(m => m.UnitPrice * m.Quantity),
                    DeliveryFee = first.DeliveryFee ?? 0,
                    TotalCost = products.Sum(i => i.UnitPrice * i.Quantity)
                                + menus.Sum(m => m.UnitPrice * m.Quantity)
                                - (first.Discount ?? 0) + (first.DeliveryFee ?? 0),
                    Status = new OrderStatus { StatusId = first.StatusId, Status = first.Status },
                    DeliveryEta = first.DeliveryEta
                });
            }
        }

        private async Task ChangeOrderStatusAsync(AdminOrderDisplayItem item)
        {
            var statuses = await _db.OrderStatuses.OrderBy(s => s.StatusId).ToListAsync();

            int curIndex = statuses.FindIndex(s => s.Status == item.StatusText);
            if (curIndex == -1 || curIndex == statuses.Count - 1)
                return; 

            var nextStatus = statuses[curIndex + 1];

            var order = await _db.Orders
                .Include(o => o.Status)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.OrderMenuItems)
                    .ThenInclude(omi => omi.Menu)
                        .ThenInclude(m => m.MenuItems)
                            .ThenInclude(mi => mi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == item.OrderId);

            if (order != null)
            {
                var oldStatus = order.Status?.Status?.Trim().ToLower();
                var newStatusStr = nextStatus.Status?.Trim().ToLower();

                if (oldStatus != "se pregateste" && newStatusStr == "se pregateste")
                {

                    foreach (var orderItem in order.OrderItems)
                    {
                        if (orderItem.Product != null)
                        {
                            orderItem.Product.TotalQuantity -= orderItem.Quantity;
                            if (orderItem.Product.TotalQuantity < 0)
                                orderItem.Product.TotalQuantity = 0;

                            _db.Entry(orderItem.Product).State = EntityState.Modified;
                        }
                    }
                    foreach (var menuOrder in order.OrderMenuItems)
                    {
                        if (menuOrder.Menu?.MenuItems != null)
                        {
                            foreach (var mi in menuOrder.Menu.MenuItems)
                            {
                                var prod = mi.Product;
                                if (prod != null)
                                {
                                    var qty = mi.QuantityInMenu * menuOrder.Quantity;
                                    prod.TotalQuantity -= qty;
                                    if (prod.TotalQuantity < 0)
                                        prod.TotalQuantity = 0;

                                    _db.Entry(prod).State = EntityState.Modified;
                                }
                            }
                        }
                    }
                }
                order.StatusId = nextStatus.StatusId;
                await _db.SaveChangesAsync();
            }
            await LoadOrdersAsync();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
