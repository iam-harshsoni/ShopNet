using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ShopNet.Models
{
    /// <summary>
    /// Extends IdentityUser with ShopNet-specific profile fields.
    /// Always extend — never use IdentityUser directly.
    /// If you use IdentityUser directly and later need to add fields,
    /// you'd need a painful migration.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // Profile fields
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PhoneNumber2 { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        //Computed - not stored in DB
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}