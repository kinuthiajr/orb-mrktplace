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
        public required string ShopSlug {get; set;}
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

    public class ShopProfileDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string ShopSlug { get; set; }
        public DateTime CreatedAt { get; set; }
        public SellerBasicInfoDto Seller { get; set; }
        public List<ProductDto> FeaturedProducts { get; set; }
        public int TotalProducts { get; set; }
    }

    public class SellerProfileDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string ShopName { get; set; }
        public string ShopDescription { get; set; }
        public DateTime JoinedDate { get; set; }
        public ShopProfileDto Shop { get; set; }
        public int TotalProducts { get; set; }
        
    }

    public class SellerBasicInfoDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string ShopName { get; set; }
    }
}