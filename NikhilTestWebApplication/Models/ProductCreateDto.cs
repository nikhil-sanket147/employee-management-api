namespace NikhilTestWebApplication.Models
{
    public class ProductCreateDto
    {
        public Guid UserId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal Price { get; set; }
    }
}
