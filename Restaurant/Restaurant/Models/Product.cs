using System;
using System.Collections.Generic;

namespace Restaurant.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string PortionSize { get; set; } = null!;

    public decimal TotalQuantity { get; set; }

    public int CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductPhoto> ProductPhotos { get; set; } = new List<ProductPhoto>();

    public virtual ICollection<Allergen> Allergens { get; set; } = new List<Allergen>();
    public string PhotoUrl => ProductPhotos?.FirstOrDefault()?.PhotoUrl ?? "images/default_product.jpg";
    public bool IsAvailable => TotalQuantity > 0;
    public string AvailabilityText => TotalQuantity > 0 ? "" : "indisponibil";
    public string AllergensText => Allergens != null && Allergens.Any()
    ? string.Join(", ", Allergens.Select(a => a.Name))
    : "Fără alergeni";

}
