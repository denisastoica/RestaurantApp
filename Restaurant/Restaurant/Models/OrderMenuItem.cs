using Restaurant.Models;

public partial class OrderMenuItem
{
    public int OrderId { get; set; }
    public int MenuId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public virtual Order Order { get; set; } = null!;
    public virtual Menu Menu { get; set; } = null!;
}