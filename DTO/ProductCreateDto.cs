using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orb.API.DTO
{
    public class ProductCreateDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required decimal Price { get; set; }
        public required int StockQuantity { get; set; }
    }

    public class ProductDto
    {
        public Guid Id {get;set;}
        public required string Name { get; set; }
        public string Description { get; set; }
        public required decimal Price { get; set; }
        public required int StockQuantity { get; set; }
        public string ProductSlug { get; set; }
        public string FullSlug {get; set;}
        public Guid ShopId {get;set;}
        public string ShopName {get;set;}
    }
}