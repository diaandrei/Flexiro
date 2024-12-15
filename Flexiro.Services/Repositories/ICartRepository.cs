using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;

namespace Flexiro.Services.Repositories
{
    public interface ICartRepository
    {

        Task<Cart> GetCartByUserIdAsync(string userId);
        Task<Cart> CreateNewCartAsync(string userId);
        Task<CartItem> AddOrUpdateCartItemAsync(Cart cart, CartItemRequestModel itemRequest, Product product);
        Task UpdateCartTotalsAsync(Cart cart);

        Task<MainCartModel> GetCartAsync(string userId);

        Task<CartItem?> UpdateCartItemQuantityAsync(int cartItemId, int quantity);
        Task<CartItem?> RemoveItemFromCartAsync(int cartItemId);
        Task<CartSummaryResponseModel> GetCartSummaryAsync(string userId);
        Task<bool> ClearCartAsync(string userId);
        Task<decimal> GetProductTotalAsync(List<int> productIds);
        Task<bool> IsCartExistAsync(int? userId);
        Task<decimal> GetTotalCartPriceAsync(int? userId);
        Task<int?> GetCartItemCountAsync(string userId);
    }
}