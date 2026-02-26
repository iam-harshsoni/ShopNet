using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ShopNet.Models
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decima(18,2)")]
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => UnitPrice * Quantity;
    }
}