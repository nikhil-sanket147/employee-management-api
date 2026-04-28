using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NikhilTestWebApplication.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();   // PRIMARY KEY

        public int UserId { get; set; }

        [NotMapped]
        public string? UserName { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }
    }
}