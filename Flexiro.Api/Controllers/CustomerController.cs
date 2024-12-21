using Flexiro.Application.DTOs;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Flexiro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IShopService _shopService;
        private readonly IProductService _productService;
        private readonly IReviewService _reviewService;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IShippingService _shippingService;

        public CustomerController(IShopService shopService, IProductService productService, IReviewService reviewService, ICartService cartService, IOrderService orderService, IShippingService shippingService, IBlobStorageService blobStorageService)
        {
            _shopService = shopService;
            _productService = productService;
            _reviewService = reviewService;
            _cartService = cartService;
            _orderService = orderService;
            _shippingService = shippingService;
            _blobStorageService = blobStorageService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetCustomerDashboardAsync()
        {
            // Fetch active shops
            var shopServiceResponse = await _shopService.GetActiveShopsAsync();

            var shopSummaries = shopServiceResponse.Content?.Select(shop => new ShopSummaryResponse
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                ShopLogo = shop.ShopLogo,
                ShopDescription = shop.ShopDescription
            }).ToList() ?? new List<ShopSummaryResponse>();

            // Fetch sale products
            var saleProductsResponse = await _productService.GetSaleProductsAsync();
            var saleProducts = saleProductsResponse.Content?.Select(product => new ProductSaleResponseDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                MainImage = product.MainImage,
                OriginalPrice = product.OriginalPrice,
                DiscountedPrice = product.DiscountedPrice,
                DiscountPercentage = product.DiscountPercentage,
                SaleEndDate = product.SaleEndDate,
                StockQuantity = product.StockQuantity,
                TotalSold = product.TotalSold
            }).ToList() ?? new List<ProductSaleResponseDto>();

            var topRatedAffordableProductsResponse = await _productService.GetTopRatedAffordableProductsAsync();
            var topRatedAffordableProducts = topRatedAffordableProductsResponse.Select(product => new ProductTopRatedDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                PricePerItem = product.PricePerItem,
                ShopImage = product.ShopImage
            }).ToList();

            // Create a combined response model
            var response = new CustomerDashboardResponse
            {
                Shops = shopSummaries,
                SaleProducts = saleProducts,
                TopRatedAffordableProducts = topRatedAffordableProducts,
                Title = "Customer Dashboard Data Retrieved Successfully",
                Description = "All active shops, sale products, and cheaper products have been retrieved."
            };

            return Ok(new ResponseModel<CustomerDashboardResponse>
            {
                Success = true,
                Content = response,
                Title = response.Title,
                Description = response.Description
            });
        }

        [HttpGet("shops/{shopId}/products")]
        public async Task<IActionResult> GetProductsByShopId(int shopId, [FromQuery] string? userId)
        {
            var response = await _productService.GetProductsByShopIdAsync(shopId, userId!);

            if (response.Success)
            {
                return Ok(response);
            }

            return NotFound(response);
        }

        [HttpGet("product/details/{productId}")]
        public async Task<IActionResult> GetProductDetails(int productId, [FromQuery] string userId)
        {
            var result = await _productService.GetProductDetailsByIdAsync(productId, userId);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }

        [HttpPost("product/review")]
        public async Task<IActionResult> AddOrUpdateReview([FromForm] AddReviewRequestDto reviewDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            var result = await _reviewService.AddOrUpdateReviewAsync(reviewDto);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPost("add-product-to-cart")]
        public async Task<IActionResult> AddItemToCartAsync([FromBody] MultiCartItemRequestModel requestModel, string userId)
        {
            var response = await _cartService.AddItemToCartAsync(requestModel, userId);

            if (!response.Success)
            {
                return BadRequest(new { response.Title, response.Description, response.ExceptionMessage });
            }

            return Ok(new { response.Title, response.Description, Items = response.Content });
        }


        [HttpPut("UpdateCartItemQuantity")]
        public async Task<IActionResult> UpdateCartItemQuantity(UpdateCartDto cartData)
        {
            var result = await _cartService.UpdateCartItemQuantityAsync(cartData.CartItemId, cartData.Quantity, cartData.UserId);

            if (!result.Success)
            {
                if (result.Title == "Cart Item Not Found" || result.Title == "Product Not Found")
                    return NotFound(new { result.Title, result.Description });

                if (result.Title == "Invalid Quantity" || result.Title == "Invalid User ID" || result.Title == "Insufficient Stock")
                    return BadRequest(new { result.Title, result.Description });

                return StatusCode(StatusCodes.Status500InternalServerError, new { result.Title, result.Description });
            }

            // Return success response with updated cart item details
            return Ok(new
            {
                result.Title,
                result.Description,
                Data = result.Content
            });
        }

        [HttpDelete("RemoveItemFromCart")]
        public async Task<IActionResult> RemoveItemFromCart([FromQuery] int cartItemId, [FromQuery] string userId)
        {
            var result = await _cartService.RemoveItemFromCartAsync(cartItemId, userId);

            if (!result.Success)
            {
                if (result.Title == "Cart Item Not Found")
                    return NotFound(new { result.Title, result.Description });

                if (result.Title == "Invalid User ID")
                    return BadRequest(new { result.Title, result.Description });

                return StatusCode(StatusCodes.Status500InternalServerError, new { result.Title, result.Description });
            }

            return Ok(new
            {
                result.Title,
                result.Description,
                Data = result.Content
            });
        }

        [HttpGet("cart")]
        public async Task<IActionResult> GetCart(string userId)
        {
            var result = await _cartService.GetCartAsync(userId);

            if (result.Success)
            {
                return Ok(result);
            }
            return NotFound(result);
        }

        [HttpPost("add-product-wishlist")]
        public async Task<IActionResult> AddProductToWishlist([FromBody] AddToWishlistRequest request)
        {
            var response = await _productService.AddProductToWishlistAsync(request.ProductId, request.UserId, int.Parse(request.ShopId));

            if (!response.Success)
            {
                return BadRequest(new
                {
                    response.Title,
                    response.Description,
                    response.ExceptionMessage
                });
            }

            return Ok(response);
        }

        [HttpDelete("remove-product-wishlist")]
        public async Task<IActionResult> RemoveProductFromWishlist([FromQuery] int productId, [FromQuery] string userId, [FromQuery] string shopId)
        {
            var response = await _productService.RemoveProductFromWishlistAsync(productId, userId, int.Parse(shopId));

            if (!response.Success)
            {
                return BadRequest(new
                {
                    response.Title,
                    response.Description,
                    response.ExceptionMessage
                });
            }

            return Ok(response);
        }

        [HttpGet("cart/summary")]
        public async Task<IActionResult> GetCartSummary([FromQuery] CartSummaryRequestModel request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest(new { Success = false, Message = "User ID is required." });
            }
            var result = await _cartService.GetCartSummaryAsync(request.UserId);

            if (!result.Success)
            {
                return BadRequest(new { Success = result.Success, Title = result.Title, Description = result.Description });
            }

            return Ok(new { Success = result.Success, Title = result.Title, Description = result.Description, Content = result.Content });
        }


        [HttpPost("order/place")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest placeOrderRequest)
        {
            if (placeOrderRequest == null! || string.IsNullOrEmpty(placeOrderRequest.UserId))
            {
                return BadRequest("Invalid order request.");
            }

            // Pass the userId and shipping address details to the service
            var result = await _orderService.PlaceOrderAsync(placeOrderRequest.UserId, placeOrderRequest.ShippingAddress, placeOrderRequest.PaymentMethod);

            if (result.Success)
            {
                var clearCartResponse = await _cartService.ClearCartAsync(placeOrderRequest.UserId);

                return Ok(result);
            }

            return StatusCode(500, result);
        }

        [HttpGet("GetAddressBook")]
        public async Task<IActionResult> GetAddressBookByUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new
                {
                    Success = false,
                    Title = "Invalid User ID",
                    Description = "User ID cannot be null or empty."
                });
            }

            var result = await _shippingService.GetAddressBookByUserIdAsync(userId);

            if (!result.Success)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Title = result.Title,
                    Description = result.Description,
                    ExceptionMessage = result.ExceptionMessage
                });
            }

            return Ok(result);
        }

        [HttpPost("clearCart")]
        public async Task<IActionResult> ClearCart(string userId)
        {
            var result = await _cartService.ClearCartAsync(userId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result);
            }
        }

        [HttpGet("cart/item-count/{userId}")]
        public async Task<IActionResult> GetCartItemCount(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { success = false, message = "Invalid user ID." });
            }

            var itemCount = await _cartService.GetCartItemCountAsync(userId);

            if (itemCount.HasValue)
            {
                return Ok(new { success = true, totalItems = itemCount.Value });
            }

            return NotFound(new { success = false, message = "Cart not found or empty." });
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetCustomerOrders(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var orders = await _orderService.GetOrdersByCustomerAsync(userId);

            if (orders == null! || !orders.Any())
                return NotFound(new { message = "No orders found for the customer." });

            return Ok(orders);
        }

        [HttpGet("wishlist-products/{userId}")]
        public async Task<IActionResult> GetWishlistProducts(string userId)
        {
            var response = await _productService.GetWishlistProductsByUserAsync(userId);

            if (!response.Success)
            {
                return BadRequest(new
                {
                    response.Title,
                    response.Description,
                    response.ExceptionMessage
                });
            }

            return Ok(response);
        }
    }
}
