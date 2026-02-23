using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ShopNet.Models
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }
    public class Order : BaseEntity
    {
        public string UserId { get; set; } = string.Empty; // Will link to identity user later
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmout { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

        // Shipping Address - denormalized intentionally
        // (address at time of order, not current user address)
        [Required]
        public string ShippingName { get; set; } = string.Empty;

        [Required]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        public string ShippingCity { get; set; } = string.Empty;

        [Required]
        public string ShippingPinCode { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}