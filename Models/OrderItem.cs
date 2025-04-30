using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Orb.API.Models
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public int Quantity { get; set; } = 1;
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
        
        // Foreign key to order
        [Required]
        public Guid OrderId { get; set; }
        
        // Navigation property
        [ForeignKey("OrderId")]
        public Order Order { get; set; }
        
        // Foreign key to product
        [Required]
        public Guid ProductId { get; set; }
        
        // Navigation property
        [ForeignKey("ProductId")]
        public Product Product { get; set; }
        
        // Store product details at time of purchase (in case product changes later)
        [Required]
        public string ProductName { get; set; }
        
        public string ProductDescription { get; set; }
    }
}