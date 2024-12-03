using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Flexiro.Services.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CartService> logger, ICartRepository cartRepository, IProductRepository productRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }
        public async Task<ResponseModel<List<CartItem>>> AddItemToCartAsync(MultiCartItemRequestModel requestModel, string userId)
        {
            var response = new ResponseModel<List<CartItem>>();
            var addedItems = new List<CartItem>();

            try
            {
                // Get the user's cart or create a new one
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);

                if (cart == null!)
                {
                    cart = await _cartRepository.CreateNewCartAsync(userId);
                }

                foreach (var itemRequest in requestModel.Items)
                {
                    // Validate product existence and stock
                    var product = await _productRepository.GetProductByIdAsync(itemRequest.ProductId);

                    if (product == null!)
                    {
                        response = new ResponseModel<List<CartItem>>
                        {
                            Success = false,
                            Title = "Product Not Found",
                            Description = $"Product with ID '{itemRequest.ProductId}' does not exist."
                        };
                        return response;
                    }
                    if (product.StockQuantity < itemRequest.Quantity || itemRequest.Quantity <= 0)
                    {
                        response = new ResponseModel<List<CartItem>>
                        {
                            Success = false,
                            Title = product.StockQuantity < itemRequest.Quantity ? "Insufficient Stock" : "Invalid Quantity",
                            Description = product.StockQuantity < itemRequest.Quantity
                                ? $"Only {product.StockQuantity} units available for the requested product."
                                : "Quantity must be greater than zero."
                        };

                        return response;
                    }

                    // Add or update cart item
                    var updatedCartItem = await _cartRepository.AddOrUpdateCartItemAsync(cart, itemRequest, product);
                    addedItems.Add(updatedCartItem);
                }

                // Update cart totals
                await _cartRepository.UpdateCartTotalsAsync(cart);

                response.Success = true;
                response.Content = addedItems;
                response.Title = "Item Added Successfully";
                response.Description = "The item has been added to your cart.";
            }
            catch (Exception ex)
            {
                response = new ResponseModel<List<CartItem>>
                {
                    Success = false,
                    Title = "Error Adding Item to Cart",
                    Description = "An error occurred while adding the item to the cart.",
                    ExceptionMessage = ex.Message
                };
                _logger.LogError(ex, "Error occurred while adding item to cart for user ID: {UserId}", userId);
            }

            return response;
        }

        public async Task<bool> IsCartExistAsync(int? userId)
        {
            try
            {
                return await _cartRepository.IsCartExistAsync(userId);
            }
            catch (Exception ex)
            {
                // Log the error if needed
                _logger.LogError(ex, "Error checking if cart exists for user ID: {UserId}", userId);
                return false;
            }
        }

        public async Task<ResponseModel<MainCartModel>> GetCartAsync(string userId)
        {
            var response = new ResponseModel<MainCartModel>();

            try
            {
                // Call repository function to get the cart data
                var mainCartModel = await _cartRepository.GetCartAsync(userId);

                // Check if the cart was found
                if (mainCartModel == null!)
                {
                    response.Success = false;
                    response.Title = "Cart Not Found";
                    response.Description = $"Cart with user ID '{userId}' does not exist.";
                    return response;
                }

                // Populate response with cart details
                response.Success = true;
                response.Content = mainCartModel;
                response.Title = "Cart Retrieved Successfully";
                response.Description = "The cart details have been retrieved.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Retrieving Cart";
                response.Description = "An error occurred while retrieving the cart.";
                response.ExceptionMessage = ex.Message;
                _logger.LogError(ex, "Error occurred while retrieving cart with ID: {CartId}", userId);
            }

            return response;
        }

        public async Task<decimal> GetProductTotalAsync(List<int> productIds)
        {
            try
            {
                var totalProductPrice = await _cartRepository.GetProductTotalAsync(productIds);

                return totalProductPrice;
            }
            catch (Exception ex)
            {
                // Log error if necessary
                _logger.LogError(ex, "Error occurred while calculating total price for product IDs: {ProductIds}", string.Join(", ", productIds));
                return 0; // Return a default value or handle as necessary
            }
        }

        public async Task<decimal> GetTotalCartPriceAsync(int? userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null.");
            }

            try
            {
                // Call repository function to get the total price of the cart
                var totalCartPrice = await _cartRepository.GetTotalCartPriceAsync(userId);

                return totalCartPrice;
            }
            catch (Exception ex)
            {
                // Log error if necessary
                _logger.LogError(ex, "Error occurred while retrieving total cart price for user ID: {UserId}", userId);
                return 0; // Return a default value or handle as necessary
            }
        }

        public async Task<ResponseModel<CartItem>> UpdateCartItemQuantityAsync(int cartItemId, int quantity, string? userId)
        {
            var response = new ResponseModel<CartItem>();

            // Validate inputs
            if (userId == null)
            {
                response.Success = false;
                response.Title = "Invalid User ID";
                response.Description = "User ID cannot be null.";
                return response;
            }

            if (quantity <= 0)
            {
                response.Success = false;
                response.Title = "Invalid Quantity";
                response.Description = "Quantity must be greater than zero.";
                return response;
            }

            try
            {
                // Call the repository function to update the cart item quantity
                var cartItem = await _cartRepository.UpdateCartItemQuantityAsync(cartItemId, quantity);

                if (cartItem == null)
                {
                    response.Success = false;
                    response.Title = "Error Updating Cart Item";
                    response.Description = "An error occurred while updating the cart item (either item not found or insufficient stock).";
                }
                else
                {
                    response.Success = true;
                    response.Content = cartItem;
                    response.Title = "Quantity Updated Successfully";
                    response.Description = "The cart item quantity has been updated.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Updating Cart Item";
                response.Description = "An error occurred while updating the cart item.";
                response.ExceptionMessage = ex.Message;
                _logger.LogError(ex, "Error occurred while updating cart item ID: {CartItemId} for user ID: {UserId}", cartItemId, userId);
            }

            return response;
        }

        public async Task<ResponseModel<CartItem>> RemoveItemFromCartAsync(int cartItemId, string? userId)
        {
            var response = new ResponseModel<CartItem>();

            if (userId == null)
            {
                response.Success = false;
                response.Title = "Invalid User ID";
                response.Description = "User ID cannot be null.";
                return response;
            }

            try
            {
                // Call the repository function to remove the item from the cart
                var cartItem = await _cartRepository.RemoveItemFromCartAsync(cartItemId);

                if (cartItem == null)
                {
                    response.Success = false;
                    response.Title = "Cart Item Not Found";
                    response.Description = $"No cart item found with ID '{cartItemId}'.";
                }
                else
                {
                    response.Success = true;
                    response.Content = cartItem;
                    response.Title = "Item Removed Successfully";
                    response.Description = "The item has been removed from the cart.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error Removing Cart Item";
                response.Description = "An error occurred while removing the cart item.";
                response.ExceptionMessage = ex.Message;
                _logger.LogError(ex, "Error occurred while removing cart item ID: {CartItemId} for user ID: {UserId}", cartItemId, userId);
            }

            return response;
        }

        public async Task<ResponseModel<string>> ClearCartAsync(string userId)
        {
            var response = new ResponseModel<string>();

            try
            {
                // Call repository function to clear the cart
                bool success = await _cartRepository.ClearCartAsync(userId);

                if (success)
                {
                    // Prepare the response indicating the cart has been successfully cleared
                    response.Success = true;
                    response.Content = "The cart has been cleared successfully.";
                    response.Title = "Cart Cleared";
                    response.Description = "All items have been removed from the cart.";
                }
                else
                {
                    // Prepare the response when cart is not found or an error occurs in the repository
                    response.Success = false;
                    response.Title = "Cart Not Found";
                    response.Description = $"No cart found for UserID '{userId}'.";
                }
            }
            catch (Exception ex)
            {
                // Log error and prepare error response
                response.Success = false;
                response.Title = "Error Clearing Cart";
                response.Description = "An error occurred while clearing the cart.";
                response.ExceptionMessage = ex.Message;
                _logger.LogError(ex, "Error occurred while clearing cart for UserID: {UserId}", userId);
            }

            return response;
        }

        public async Task<ResponseModel<CartSummaryResponseModel>> GetCartSummaryAsync(string UserId)
        {
            var response = new ResponseModel<CartSummaryResponseModel>();

            try
            {
                // Call repository function to get the cart summary
                var cartSummary = await _cartRepository.GetCartSummaryAsync(UserId.ToString());

                if (cartSummary == null!)
                {
                    response.Success = false;
                    response.Title = "Cart Not Found";
                    response.Description = "No active cart found for the user.";
                    return response;
                }

                // Prepare the success response
                response.Success = true;
                response.Content = cartSummary;
                response.Title = "Cart Summary Retrieved Successfully";
                response.Description = "The cart summary has been retrieved.";
            }
            catch (Exception ex)
            {
                // Log error and prepare error response
                response.Success = false;
                response.Title = "Error Retrieving Cart Summary";
                response.Description = "An error occurred while retrieving the cart summary.";
                response.ExceptionMessage = ex.Message;
                _logger.LogError(ex, "Error occurred while retrieving cart summary for user ID: {UserId}", UserId);
            }

            return response;
        }

        public async Task<int?> GetCartItemCountAsync(string userId)
        {
            try
            {
                // Fetch the total item count for the user's cart via the repository
                return await _cartRepository.GetCartItemCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving cart item count for user ID: {UserId}", userId);
                return null;
            }
        }
    }
}