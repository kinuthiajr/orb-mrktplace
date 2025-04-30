using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Orb.API.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        // Shipping information
        [Required]
        public string ShippingAddress { get; set; }
        
        // Foreign key to customer
        [Required]
        public string CustomerId { get; set; }
        
        // Navigation property
        [ForeignKey("CustomerId")]
        public ApplicationUser Customer { get; set; }
        
        // Foreign key to seller
        [Required]
        public string SellerId { get; set; }
        
        // Navigation property
        [ForeignKey("SellerId")]
        public ApplicationUser Seller { get; set; }
        
        // Navigation property to order items
        public ICollection<OrderItem> OrderItems { get; set; }
    }
}