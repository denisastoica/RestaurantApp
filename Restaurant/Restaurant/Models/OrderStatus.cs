using System.Collections.Generic;

namespace Restaurant.Models
{
    public partial class OrderStatus
    {
        public int StatusId { get; set; }
        public string Status { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
