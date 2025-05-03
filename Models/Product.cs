using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Orb.API.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        public int StockQuantity { get; set; }

        [Required]
        public string Slug {get; set;}
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key to shop
        [Required]
        public Guid ShopId { get; set; }
        
        // Navigation property to shop
        [ForeignKey("ShopId")]
        public Shop Shop { get; set; }
        
    }
    
}