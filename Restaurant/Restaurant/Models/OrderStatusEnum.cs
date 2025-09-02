using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Models
{
    public enum OrderStatusEnum
    {
        Registered,     
        Preparing,  
        OutForDelivery, 
        Delivered,      
        Canceled       
    }
}
