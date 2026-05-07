using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
namespace NikhilTestWebApplication.Models
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();   // PRIMARY KEY

        //foreign key
        public Guid UserId { get; set; }

        //navigation property
        [JsonIgnore]
        public User? User { get; set; }

        public string ProductName { get; set; } = string.Empty;

        [NotMapped]
        public string? UserName { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }
    }
}