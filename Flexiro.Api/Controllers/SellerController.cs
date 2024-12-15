using Flexiro.Application.DTOs;
using Flexiro.Contracts.Requests;
using Flexiro.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Flexiro.API.Controllers
{
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

            if (result.Success)
            {
                return Ok(result);
            }

            // If there's an error, return the failure response
            return BadRequest(result);
        }

        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var result = await _productService.GetAllCategoryNamesAsync();
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("getallproducts/{shopId}")]
        public async Task<IActionResult> GetAllProducts(int shopId)
        {
            var response = await _productService.GetAllProductsAsync(shopId);

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

        [HttpPut("updateshop")]
        public async Task<IActionResult> UpdateShop([FromForm] UpdateShopRequest updateShopRequest)
        {
            var response = await _shopService.UpdateShopAsync(updateShopRequest);

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

        [HttpPut("product/status")]
        public async Task<IActionResult> ChangeProductStatus(StatusDto newStatus)
        {
            var response = await _productService.ChangeProductStatusAsync(newStatus.ProductId, newStatus.NewStatus);

            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrdersByShop(int shopId)
        {
            var orders = await _orderService.GetGroupedOrdersByShopAsync(shopId);

            if (orders == null! || (!orders.NewOrders.Any() && !orders.DeliveredOrders.Any() && !orders.AllOrders.Any()))
            {
                return NotFound(new { success = false, message = "No orders found for this shop." });
            }

            return Ok(new
            {
                success = true,
                newOrders = orders.NewOrders,
                pendingOrders = orders.PendingOrders,
                processingOrders = orders.ProcessingOrders,
                shippedOrders = orders.ShippedOrders,
                deliveredOrders = orders.DeliveredOrders,
                canceledOrders = orders.CanceledOrders,
                returnedOrders = orders.ReturnedOrders,
                completedOrders = orders.CompletedOrders,
                allOrders = orders.AllOrders
            });
        }

        [HttpPut("ChangeShopStatus")]
        public async Task<IActionResult> ChangeShopStatus([FromBody] ShopStatus newStatus)
        {
            var result = await _shopService.ChangeShopSellerStatusAsync(newStatus);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("order/update-status")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid request data." });

            var result = await _orderService.UpdateOrderStatusAsync(request);

            if (!result)
                return NotFound(new { success = false, message = "Order not found or status update failed." });

            return Ok(new { success = true, message = "Order status updated successfully." });
        }
    }
}