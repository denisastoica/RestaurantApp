using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public partial class Order
    {
        public int OrderId { get; set; }
        public Guid OrderCode { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public int StatusId { get; set; }
        public DateTime? DeliveryEta { get; set; }
        public decimal Discount { get; set; }
        public decimal DeliveryFee { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<OrderMenuItem> OrderMenuItems { get; set; } = new List<OrderMenuItem>();
        public virtual User User { get; set; } = null!;
        public virtual OrderStatus Status { get; set; }
    }
}
