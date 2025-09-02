using System;
using System.Collections.Generic;

namespace Restaurant.Models
{
    public partial class MenuPhoto
    {
        public int MenuPhotoId { get; set; }

        public int MenuId { get; set; }

        public string PhotoUrl { get; set; } = null!; 

        public virtual Menu Menu { get; set; } = null!;
    }
}
