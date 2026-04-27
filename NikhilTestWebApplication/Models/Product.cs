using Microsoft.EntityFrameworkCore;

namespace NikhilTestWebApplication.Models
{
    public class Product
    {
        public int userId { get; set; }
        public string? userName { get; set; } // optional, not mapped to DB


        [Precision(18,2)]
        public decimal price { get; set; }
    }
}
