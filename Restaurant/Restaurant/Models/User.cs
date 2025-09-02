    using System;
    using System.Collections.Generic;

    namespace Restaurant.Models;

    public partial class User
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Phone { get; set; } = null!;

        public string? DeliveryAddress { get; set; }

        public byte[] PasswordHash { get; set; } = null!;

        public string Role { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
