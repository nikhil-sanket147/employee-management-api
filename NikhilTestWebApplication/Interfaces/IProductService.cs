using NikhilTestWebApplication.Models;

namespace NikhilTestWebApplication.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProducts();
        Task<Product?> GetProductById(Guid id);
        Task AddProduct(Product product);
        Task<bool> UpdateProduct(int id, Product product);
        Task<bool> DeleteProduct(int id);
    }
}
