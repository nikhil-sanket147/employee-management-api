using Microsoft.AspNetCore.Mvc;
using NikhilTestWebApplication.Interfaces;
using NikhilTestWebApplication.Models;
using NikhilTestWebApplication.Services;

namespace NikhilTestWebApplication.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        private readonly UserServiceClient _userClient;

        public ProductController(IProductService productService, UserServiceClient userClient)
        {
            _productService = productService;
            _userClient = userClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProducts();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto dto)
        {
            var product = new Product
            {
                UserId = dto.UserId,
                ProductName = dto.ProductName,
                Price = dto.Price
            };

            await _productService.AddProduct(product);

            // Fetch user
            var user = await _userClient.GetUserById(product.UserId);

            // Enrich response
            product.UserName = user?.Username;

            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Product product)
        {
            var updated = await _productService.UpdateProduct(id, product);
            if (!updated)
                return NotFound();

            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _productService.DeleteProduct(id);
            if (!deleted)
                return NotFound();

            return Ok();
        }
    }
}
