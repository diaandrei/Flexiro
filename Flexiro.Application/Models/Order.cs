﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flexiro.Application.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int ShippingAddressId { get; set; }
        public virtual ShippingAddress ShippingAddress { get; set; }

        public int BillingAddressId { get; set; }
        public virtual ShippingAddress BillingAddress { get; set; }

        public string OrderNumber { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ItemsTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ShippingCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }

        public DateTime? DeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<OrderDetails> OrderDetails { get; set; }
        public virtual Payment Payment { get; set; }
    }
}