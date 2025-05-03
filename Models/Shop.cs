using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Orb.API.Models
{
    public class Shop
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public required string Name { get; set; }
        
        [Required]
        public required string Description { get; set; }

        [Required]
        public required string Location {get; set;}

        [Required]
        public string ShopSlug {get; set;}
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key to seller one-one rship
        [Required]
        public string SellerId { get; set; }
        
        // Navigation property to seller
        [ForeignKey("SellerId")]
        public ApplicationUser Seller { get; set; }
        
        // Navigation property to products
        public ICollection<Product> Products { get; set; }
    }
}