using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Flexiro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IShopService _shopService;
        private readonly IOrderService _orderService;
        private readonly IReviewService _reviewService;
        private readonly IBlobStorageService _blobStorageService;
        public AdminController(IShopService shopService, IOrderService orderService, IReviewService reviewService, IBlobStorageService blobStorageService)
        {
            _shopService = shopService;
            _orderService = orderService;
            _reviewService = reviewService;
            _blobStorageService = blobStorageService;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<ResponseModel<AdminDashboardDto>>> GetDashboardData()
        {
            var response = new ResponseModel<AdminDashboardDto>();

            try
            {
                // Retrieve all shop lists
                var activeShopsResponse = await _shopService.GetActiveShopsAsync();
                var pendingShopsResponse = await _shopService.GetPendingShopsAsync();
                var inactiveShopsResponse = await _shopService.GetInactiveShopsAsync();
                var allShopsResponse = await _shopService.GetAllShopsAsync();

                if (!activeShopsResponse.Success ||
                    !pendingShopsResponse.Success ||
                    !inactiveShopsResponse.Success ||
                    !allShopsResponse.Success)
                {
                    response.Success = false;
                    response.Title = "Error Retrieving Dashboard Data";
                    response.Description = "One or more shop lists could not be retrieved.";
                    return BadRequest(response);
                }

                var dashboardData = new AdminDashboardDto
                {
                    ActiveShops = await GetShopDetails(activeShopsResponse.Content),
                    PendingShops = await GetShopDetails(pendingShopsResponse.Content),
                    InactiveShops = await GetShopDetails(inactiveShopsResponse.Content),
                    AllShops = await GetShopDetails(allShopsResponse.Content)
                };

                response.Success = true;
                response.Content = dashboardData;
                response.Title = "Dashboard Data Retrieved Successfully";
                response.Description = "Shop lists have been retrieved successfully.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Dashboard Data";
                response.Description = "An error occurred while retrieving the dashboard data.";
                response.ExceptionMessage = ex.Message;
                return StatusCode(500, response); // Internal Server Error
            }

            return Ok(response);
        }

        private async Task<IList<ShopDetailsDto>> GetShopDetails(IList<Shop> shops)
        {
            var shopDetailsList = new List<ShopDetailsDto>();

            foreach (var shop in shops)
            {
                var totalEarnings = await _orderService.GetTotalEarningsByShopAsync(shop.ShopId);
                var totalOrders = await _orderService.GetTotalOrdersByShopAsync(shop.ShopId);

                // Get the average rating 
                var productRatingsResponse = await _reviewService.GetAverageRatingByShopIdAsync(shop.ShopId);

                double averageRating = 0.0;

                if (productRatingsResponse.Success && !string.IsNullOrEmpty(productRatingsResponse.Content))
                {
                    // Try to parse the average rating
                    if (double.TryParse(productRatingsResponse.Content, out double parsedRating))
                    {
                        averageRating = parsedRating;
                    }
                }

                string ratingScore = averageRating >= 4.0 ? "Good" :
                               averageRating >= 3.0 ? "Average" : "Poor";

                shopDetailsList.Add(new ShopDetailsDto
                {
                    ShopId = shop.ShopId,
                    ShopName = shop.ShopName,
                    AdminStatus = shop.AdminStatus,
                    OwnerName = shop.OwnerName,
                    ShopDescription = shop.ShopDescription,
                    ShopLogo = shop.ShopLogo,
                    TotalEarnings = totalEarnings,
                    TotalOrders = totalOrders,
                    AverageRating = averageRating,
                    RatingScore = ratingScore
                });
            }

            return shopDetailsList;
        }

        [HttpPut("change-shop-status")]
        public async Task<IActionResult> ChangeShopStatus([FromBody] ShopAdminStatusRequest request)
        {
            var response = await _shopService.ChangeShopStatusByAdminAsync(request.ShopId, request.NewStatus);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}