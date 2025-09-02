using System;
using System.Collections.Generic;

namespace Restaurant.Models
{
    // Clasa DTO modificată (Restaurant.Models)

    public class OrderWithDetailsDto
    {
        public int OrderId { get; set; }
        public Guid OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryEta { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;

        public int StatusId { get; set; }
        public string Status { get; set; } = string.Empty;

        public decimal? DeliveryFee { get; set; }  // nullable
        public decimal? Discount { get; set; }     // nullable

        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal? ProductQuantity { get; set; }
        public decimal? ProductUnitPrice { get; set; }

        public int? MenuId { get; set; }
        public string? MenuName { get; set; }
        public decimal? MenuQuantity { get; set; }
        public decimal? MenuUnitPrice { get; set; }

        public List<ProductDto> Products { get; set; } = new();
        public List<MenuDto> Menus { get; set; } = new();
    }

    public class ProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class MenuDto
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

}
