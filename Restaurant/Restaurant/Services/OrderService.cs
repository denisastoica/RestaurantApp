using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;

namespace Restaurant.Services
{
    public class OrderService
    {
        private readonly DbContextOptions<RestaurantDbContext> _dbOptions;
        private readonly ConfigurationService _config;

        public OrderService(DbContextOptions<RestaurantDbContext> dbOptions, ConfigurationService config)
        {
            _dbOptions = dbOptions;
            _config = config;
        }

        public async Task<Order> CreateOrderAsync(
    int userId,
    List<(int ProductId, int Quantity, decimal UnitPrice)> products,
    List<(int MenuId, int Quantity, decimal UnitPrice)> menus)
        {
            using var _db = new RestaurantDbContext(_dbOptions);
            var registeredStatus = await _db.OrderStatuses
       .FirstOrDefaultAsync(s => s.Status.ToLower() == "inregistrata");

            if (registeredStatus == null)
                throw new Exception("Statusul 'inregistrata' nu există în baza de date!");
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                StatusId = registeredStatus.StatusId,
                DeliveryEta = DateTime.Now.AddHours(1),
                OrderCode = Guid.NewGuid(),
                OrderItems = new List<OrderItem>(),
                OrderMenuItems = new List<OrderMenuItem>()
            };

            foreach (var p in products)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    Order = order
                });
            }
            foreach (var m in menus)
            {
                order.OrderMenuItems.Add(new OrderMenuItem
                {
                    MenuId = m.MenuId,
                    Quantity = m.Quantity,
                    UnitPrice = m.UnitPrice,
                    Order = order
                });
            }
            _db.Orders.Add(order);              
            await ApplyDiscountsAndFeesAsync(order, _db);
            await _db.SaveChangesAsync();


            return order;
        }

        public async Task UpdateOrderStatusAsync(int orderId, string newStatusString)
        {
            using var _db = new RestaurantDbContext(_dbOptions);

            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.OrderMenuItems)
                    .ThenInclude(omi => omi.Menu)
                        .ThenInclude(m => m.MenuItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                throw new Exception("Comanda nu a fost găsită.");

            var newStatus = await _db.OrderStatuses
                .FirstOrDefaultAsync(s => s.Status.ToLower() == newStatusString.Trim().ToLower());

            if (newStatus == null)
                throw new Exception($"Status invalid: {newStatusString}");

            var oldStatusString = order.Status?.Status;
            order.StatusId = newStatus.StatusId;

            if (NormalizeStatus(oldStatusString) != "se pregateste" && NormalizeStatus(newStatus.Status) == "se pregateste")
            {
                var allProductIds = order.OrderItems.Select(oi => oi.ProductId).ToList();

                foreach (var menuItem in order.OrderMenuItems)
                {
                    if (menuItem.Menu?.MenuItems != null)
                    {
                        foreach (var mi in menuItem.Menu.MenuItems)
                            allProductIds.Add(mi.ProductId);
                    }
                }
                allProductIds = allProductIds.Distinct().ToList();

                var productsDict = await _db.Products
                    .Where(p => allProductIds.Contains(p.ProductId))
                    .ToDictionaryAsync(p => p.ProductId);

                foreach (var item in order.OrderItems)
                {
                    if (productsDict.TryGetValue(item.ProductId, out var product))
                    {
                        product.TotalQuantity -= item.Quantity;
                        if (product.TotalQuantity < 0) product.TotalQuantity = 0;
                        Console.WriteLine($"Produs simplu: {product.Name}, -{item.Quantity} => {product.TotalQuantity}");
                    }
                }

                foreach (var menuItem in order.OrderMenuItems)
                {
                    if (menuItem.Menu?.MenuItems != null)
                    {
                        foreach (var mi in menuItem.Menu.MenuItems)
                        {
                            if (productsDict.TryGetValue(mi.ProductId, out var product))
                            {
                                decimal qtyToSubtract = mi.QuantityInMenu * menuItem.Quantity;
                                product.TotalQuantity -= qtyToSubtract;
                                if (product.TotalQuantity < 0) product.TotalQuantity = 0;
                                Console.WriteLine($"Produs meniu: {product.Name}, -{qtyToSubtract} => {product.TotalQuantity}");
                            }
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();
        }
        private string NormalizeStatus(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            return s.Trim().ToLower()
                .Replace("ă", "a").Replace("â", "a")
                .Replace("î", "i").Replace("ș", "s")
                .Replace("ț", "t");
        }
        private async Task ApplyDiscountsAndFeesAsync(Order order, RestaurantDbContext _db)
        {
            decimal minSumForDiscount = _config.GetDecimal("DiscountThreshold", 0m);
            decimal discountPercent = _config.GetDecimal("MenuDiscountPercent", 0m);
            decimal minSumForDeliveryFee = _config.GetDecimal("DeliveryFeeBelowAmount", 0m);
            decimal deliveryFeeAmount = _config.GetDecimal("DeliveryFee", 0m);

            int maxOrdersCount = _config.GetInt("OrderCountThreshold", 0);
            int maxOrdersIntervalMinutes = _config.GetInt("OrderTimeWindowDays", 0) * 24 * 60; 

            decimal orderTotal = order.OrderItems.Sum(i => i.UnitPrice * i.Quantity)
                                 + order.OrderMenuItems.Sum(m => m.UnitPrice * m.Quantity);

            bool applyDiscountBySum = orderTotal >= minSumForDiscount;

            var intervalStart = DateTime.UtcNow.AddMinutes(-maxOrdersIntervalMinutes);
            var recentOrdersCount = await _db.Orders
                .Where(o => o.UserId == order.UserId && o.OrderDate >= intervalStart)
                .CountAsync();

            bool applyDiscountByFrequency = recentOrdersCount > maxOrdersCount;

            if (applyDiscountBySum || applyDiscountByFrequency)
            {
                order.Discount = orderTotal * discountPercent / 100m;
            }
            else
            {
                order.Discount = 0m;
            }
            decimal finalAmount = orderTotal - order.Discount;
            if (finalAmount < minSumForDeliveryFee)
            {
                order.DeliveryFee = deliveryFeeAmount;
            }
            else
            {
                order.DeliveryFee = 0m;
            }
        }

        private decimal CalculateMenuPrice(Menu menu)
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

            return total;
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

        public async Task<List<Order>> GetOrdersByUserStoredProcAsync(int userId)
        {
            using var _db = new RestaurantDbContext(_dbOptions);

            var ordersFromDb = await _db.Orders
                .FromSqlRaw("EXEC sp_GetOrdersByUser @p0", userId)
                .ToListAsync();

            foreach (var o in ordersFromDb)
            {
                await _db.Entry(o).Reference(x => x.Status).LoadAsync();
                await _db.Entry(o).Collection(x => x.OrderItems).Query().Include(oi => oi.Product).LoadAsync();
                await _db.Entry(o).Collection(x => x.OrderMenuItems).Query().Include(omi => omi.Menu).LoadAsync();
            }
            var sortedOrders = ordersFromDb.OrderByDescending(o => o.OrderDate).ToList();

            return sortedOrders;
        }


        public async Task UpdateOrderStatusStoredProcAsync(int orderId, string newStatus)
        {
            using var _db = new RestaurantDbContext(_dbOptions);
            await _db.Database.ExecuteSqlRawAsync("EXEC sp_UpdateOrderStatus @p0, @p1", orderId, newStatus);
        }


    }
}