using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orb.API.DTO
{
    public class ShopDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        
        public required string Description { get; set; }

        public required string Location {get; set;} 
        public required string Slug {get; set;}
    }

    public class CreateShopDto
    {

        public required string Name { get; set; }
        
        public required string Description { get; set; }

        public required string Location {get; set;}  
    }

    public class ShopDetailDto : ShopDto
    {
        public List<ProductDto> Products {get; set;}
    }
}