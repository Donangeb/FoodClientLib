using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpGrpcClientLib.Models
{
    public class Order
    {
        public string Id { get; set; } = "";
        public List<OrderItem> MenuItems { get; set; } = new();
    }

    public class OrderItem
    {
        public string Id { get; set; } = "";
        public double Quantity { get; set; }
    }
}
