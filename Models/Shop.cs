using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}