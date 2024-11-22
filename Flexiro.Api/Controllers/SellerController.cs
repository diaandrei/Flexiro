using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flexiro.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SellerController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IShopService _shopService;
        private readonly IWebHostEnvironment _environment;
        public SellerController(IOrderService orderService, IProductService productService, IShopService shopService, IWebHostEnvironment environment)
        {
            _orderService = orderService;
            _productService = productService;
            _shopService = shopService;
            _environment = environment;
        }

        [HttpGet("dashboard/{shopId}")]
        public async Task<IActionResult> GetSellerDashboardData(int shopId)
        {
            // Get total new orders
            int newOrderCount = await _orderService.GetNewOrderCountByShopAsync(shopId);

            // Get total delivered orders
            var (_, deliveredOrderCount) = await _orderService.GetDeliveredOrdersByShopAsync(shopId);

            // Get total unique customers
            var (_, customerCount) = await _orderService.GetAllCustomersByShopAsync(shopId);

            var dashboardData = new
            {
                NewOrderCount = newOrderCount,
                DeliveredOrderCount = deliveredOrderCount,
                CustomerCount = customerCount
            };

            return Ok(dashboardData);
        }

        [HttpPost("product/add")]
        public async Task<IActionResult> AddProduct([FromForm] ProductCreateDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _productService.CreateProductAsync(productDto);
            string imageFolderPath = Path.Combine(_environment.WebRootPath, "uploads", "productImages", result.Content.ProductId.ToString());

            if (!Directory.Exists(imageFolderPath))
            {
                Directory.CreateDirectory(imageFolderPath);
            }

            var savedImagePaths = new List<string>();

            foreach (var imageFile in productDto.ProductImages)
            {
                if (imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var relativePath = Path.Combine("uploads", "productImages", result.Content.ProductId.ToString(), fileName);
                    var filePath = Path.Combine(_environment.WebRootPath, relativePath);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Add path to the list to update the ProductImages
                    savedImagePaths.Add(relativePath);
                }
            }

            // Update the product images paths
            await _productService.UpdateProductImagePaths(result.Content.ProductId, savedImagePaths);

            if (result.Success)
            {
                return Ok(result);
            }

            // If there's an error, return the failure response
            return BadRequest(result);
        }

        [HttpGet("getallproducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            var response = await _productService.GetAllProductsAsync();
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPut("updateproduct/{productId}")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromForm] ProductUpdateDto productDto)
        {
            var response = await _productService.UpdateProductAsync(productId, productDto);

            if (response.Success)
            {
                return Ok(response);
            }
            if (productDto.ProductImages != null && productDto.ProductImages.Any())
            {
                var savedImagePaths = await SaveProductImages(productId, productDto.ProductImages);
                await _productService.UpdateProductImagePaths(productId, savedImagePaths);
            }
            return BadRequest(response);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var result = await _productService.DeleteProductAsync(productId);

            if (result)
            {
                return Ok(new { message = $"Product with ID '{productId}' has been deleted." });
            }
            return NotFound(new { message = $"Product with ID '{productId}' does not exist." });
        }

        [HttpGet("product/search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string productName)
        {
            var response = await _productService.SearchProductsByNameAsync(productName);

            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPut("updateshop/{shopId}")]
        public async Task<IActionResult> UpdateShop(int shopId, [FromForm] UpdateShopRequest updateShopRequest)
        {

            var response = await _shopService.UpdateShopAsync(shopId, updateShopRequest);

            if (response.Success)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpGet("GetShopByOwner/{ownerId}")]
        public async Task<IActionResult> GetShopByOwnerId(string ownerId)
        {
            var result = await _shopService.GetShopByOwnerIdAsync(ownerId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("product/{productId}/status")]
        public async Task<IActionResult> ChangeProductStatus(int productId, [FromBody] ProductStatus newStatus)
        {
            var response = await _productService.ChangeProductStatusAsync(productId, newStatus);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("orders/{shopId}")]
        public async Task<IActionResult> GetOrdersByShop(int shopId)
        {
            var orders = await _orderService.GetOrdersByShopAsync(shopId);
            if (orders == null || !orders.Any())
            {
                return NotFound(new { success = false, message = "No orders found for this shop." });
            }
            return Ok(new { success = true, orders });
        }

        private async Task<List<string>> SaveProductImages(int productId, List<IFormFile> images)
        {
            var savedImagePaths = new List<string>();
            string imageFolderPath = Path.Combine(_environment.WebRootPath, "uploads", "productImages", productId.ToString());

            if (!Directory.Exists(imageFolderPath))
            {
                Directory.CreateDirectory(imageFolderPath);
            }

            foreach (var imageFile in images)
            {
                if (imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var relativePath = Path.Combine("uploads", "productImages", productId.ToString(), fileName);
                    var filePath = Path.Combine(_environment.WebRootPath, relativePath);

                    await using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    savedImagePaths.Add(relativePath);
                }
            }
            return savedImagePaths;
        }
    }
}