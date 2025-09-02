using System;
using System.Collections.Generic;

namespace Restaurant.Models;

public partial class ProductPhoto
{
    public int PhotoId { get; set; }

    public int ProductId { get; set; }

    public string? PhotoUrl { get; set; }

    public virtual Product Product { get; set; } = null!;
}
