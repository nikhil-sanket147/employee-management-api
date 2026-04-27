using Microsoft.EntityFrameworkCore;
using NikhilTestWebApplication.Data;
using NikhilTestWebApplication.Interfaces;
using NikhilTestWebApplication.Models;
using NikhilTestWebApplication.Services;

namespace NikhilTestWebApplication.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserServiceClient _userClient;

        public ProductService(
            ApplicationDbContext context,
            UserServiceClient userClient)
        {
            _context = context;
            _userClient = userClient;
        }

        public async Task<List<Product>> GetAllProducts()
        {
            // 🔥 Microservice call
            var user = await _userClient.GetUserById(1);

            return await _context.Products.ToListAsync();
        }

        public async Task<Product?> GetProductById(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.UserId == id);

            if (product == null)
                return null;

            // 🔥 Call User microservice
            if (product.UserId > 0)
            {
                try
                {
                    var user = await _userClient.GetUserById(product.UserId);
                    product.UserName = user?.Username; // optional enrichment
                }
                catch
                {
                    // Fail-safe: product should still return
                    product.UserName = null;
                }
            }

            return product;
        }

        public async Task AddProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateProduct(int id, Product product)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null)
                return false;

            existing.UserName = product.UserName;
            existing.Price = product.Price;
            existing.UserId = product.UserId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null)
                return false;

            _context.Products.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}