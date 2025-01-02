using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;

namespace Flexiro.Services.Services.Interfaces
{
    public interface ICartService
    {
        Task<ResponseModel<List<CartItem>>> AddItemToCartAsync(MultiCartItemRequestModel requestModel, string userId);
        Task<ResponseModel<MainCartModel>> GetCartAsync(string userId);
        Task<ResponseModel<CartItem>> UpdateCartItemQuantityAsync(int cartItemId, int quantity, string? userId);
        Task<ResponseModel<CartItem>> RemoveItemFromCartAsync(int cartItemId, string? userId);
        Task<ResponseModel<CartSummaryResponseModel>> GetCartSummaryAsync(string userId);
        Task<ResponseModel<string>> ClearCartAsync(string userId);
        Task<decimal> GetProductTotalAsync(List<int> productIds);
        Task<bool> IsCartExistAsync(int? userId);
        Task<decimal> GetTotalCartPriceAsync(int? userId);
        Task<int?> GetCartItemCountAsync(string userId);
        Task<ResponseModel<object>> TransferGuestCartAsync(string guestId, string userId);
    }
}