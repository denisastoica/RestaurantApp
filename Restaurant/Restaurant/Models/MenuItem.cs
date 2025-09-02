using System;
using System.Collections.Generic;

namespace Restaurant.Models;

public partial class MenuItem
{
    public int MenuId { get; set; }

    public int ProductId { get; set; }

    public decimal QuantityInMenu { get; set; }

    public virtual Menu Menu { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
