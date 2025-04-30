using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Orb.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public UserType UserType {get; set;}
        
        //Seller properties
        public string? ShopName {get; set;}
        public string? ShopDescription {get; set;}

        //Customer properties
        public string? ShippingAddress {get; set;}
    }

    public enum UserType
    {
        Customer,
        Seller
    }
}