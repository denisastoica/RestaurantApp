using Restaurant.Models;

namespace Restaurant.ViewModels

{
    public class MenuItemDisplay
    {
        public string ProductName { get; set; }
        public decimal QuantityInMenu { get; set; }
        public string PortionSize { get; set; }

        public string Unit
        {
            get
            {
                if (string.IsNullOrEmpty(PortionSize)) return "";
                var unit = new string(PortionSize.SkipWhile(char.IsDigit).ToArray()).Trim();
                return string.IsNullOrEmpty(unit) ? "g" : unit;
            }
        }
    }

    public class MenuDisplayItem
    {
        public Menu Menu { get; set; }
        public decimal CalculatedPrice { get; set; }
        public IEnumerable<MenuItemDisplay> ItemsWithQuantities { get; set; }

        public string PhotoUrl => Menu.MenuPhotos?.FirstOrDefault()?.PhotoUrl ?? "images/default_menu.jpg";
        public bool IsAvailable => Menu.MenuItems.All(mi => mi.Product != null && mi.Product.TotalQuantity > 0);
        public string AvailabilityText => Menu.MenuItems.All(mi => mi.Product != null && mi.Product.TotalQuantity > 0) ? "" : "indisponibil";


        public MenuDisplayItem(Menu menu, decimal price)
        {
            Menu = menu;
            CalculatedPrice = price;
            ItemsWithQuantities = menu.MenuItems?.Select(mi => new MenuItemDisplay
            {
                ProductName = mi.Product?.Name ?? "Unknown",
                QuantityInMenu = mi.QuantityInMenu,
                PortionSize = mi.Product?.PortionSize ?? ""
            }) ?? Enumerable.Empty<MenuItemDisplay>();
        }
    }
}
