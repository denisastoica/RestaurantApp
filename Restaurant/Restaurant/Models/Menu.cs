using System;
using System.Collections.Generic;

namespace Restaurant.Models;

public partial class Menu
{
    public int MenuId { get; set; }

    public string Name { get; set; } = null!;

    public int CategoryId { get; set; }

    public decimal DiscountPercent { get; set; }
    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<OrderMenuItem> OrderMenuItems { get; set; } = new List<OrderMenuItem>();
    public virtual ICollection<MenuPhoto> MenuPhotos { get; set; } = new List<MenuPhoto>();
    public string AvailabilityText => MenuItems.All(mi => mi.Product != null && mi.Product.TotalQuantity > 0) ? "" : "indisponibil";

}
