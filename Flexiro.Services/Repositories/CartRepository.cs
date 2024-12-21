using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Flexiro.Services.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CartRepository> _logger;

        public CartRepository(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CartRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            return (await _unitOfWork.Repository.GetQueryable<Cart>(c => c.UserId == userId)
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync())!;
        }

        public async Task<Cart> CreateNewCartAsync(string userId)
        {
            var cart = new Cart
            {
                UserId = userId,
                CartItems = new List<CartItem>(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await _unitOfWork.Repository.AddAsync(cart);
            return cart;
        }

        public async Task<CartItem> AddOrUpdateCartItemAsync(Cart cart, CartItemRequestModel itemRequest, Product product)
        {
            var originalPrice = product.PricePerItem;
            var discountAmount = originalPrice * (product.DiscountPercentage ?? 0) / 100;
            var priceAfterDiscount = originalPrice - discountAmount;

            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == itemRequest.ProductId && ci.ShopId == itemRequest.ShopId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity = itemRequest.Quantity;
                existingCartItem.PricePerUnit = originalPrice;
                existingCartItem.DiscountAmount = discountAmount * itemRequest.Quantity;
                existingCartItem.TotalPrice = priceAfterDiscount * itemRequest.Quantity;
                existingCartItem.UpdatedAt = DateTime.Now;
                _unitOfWork.Repository.Update(existingCartItem);
                return existingCartItem;
            }
            else
            {
                var newCartItem = new CartItem
                {
                    Cart = cart,
                    ProductId = product.ProductId,
                    ShopId = itemRequest.ShopId,
                    Quantity = itemRequest.Quantity,
                    PricePerUnit = originalPrice,
                    DiscountAmount = discountAmount * itemRequest.Quantity,
                    TotalPrice = priceAfterDiscount * itemRequest.Quantity,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _unitOfWork.Repository.AddAsync(newCartItem);
                return newCartItem;
            }
        }

        public async Task UpdateCartTotalsAsync(Cart cart)
        {
            cart.ItemsTotal = cart.CartItems.Sum(ci => ci.PricePerUnit * ci.Quantity);
            cart.TotalDiscount = cart.CartItems.Sum(ci => ci.DiscountAmount ?? 0);
            cart.TotalAmount = cart.CartItems.Sum(ci => ci.TotalPrice) + (cart.Tax ?? 0) + (cart.ShippingCost ?? 0);
            cart.UpdatedAt = DateTime.Now;
            await _unitOfWork.Repository.CompleteAsync();
        }

        public async Task<bool> IsCartExistAsync(int? userId)
        {
            // Check if a cart exists for the specified user and has items
            var cart = await _unitOfWork.Repository.GetQueryable<Cart>(c => c.UserId == userId.ToString())
                            .Include(c => c.CartItems) // Ensure CartItems are included for checking
                            .FirstOrDefaultAsync();

            return cart != null && cart.CartItems.Any();
        }

        public async Task<MainCartModel> GetCartAsync(string userId)
        {
            try
            {
                // Retrieve the cart by CartId
                var cart = await _unitOfWork.Repository.GetQueryable<Cart>(c => c.UserId == userId)
                                .Include(c => c.CartItems)
                                    .ThenInclude(ci => ci.Product)
                                    .ThenInclude(p => p.ProductImages)
                                .FirstOrDefaultAsync();

                if (cart == null) return null;

                // Map cart data to MainCartModel
                return new MainCartModel
                {
                    CartId = cart.CartId,
                    Items = cart.CartItems.Select(ci => new CartItemDetailModel
                    {
                        CartItemId = ci.CartItemId,
                        ProductName = ci.Product.ProductName,
                        MainImage = ci.Product.ProductImages?.FirstOrDefault() != null
                            ? ci.Product.ProductImages.First().Path
                            : string.Empty,
                        Quantity = ci.Quantity,
                        SKU = ci.Product.SKU,
                        Price = ci.PricePerUnit,
                        TotalPrice = ci.TotalPrice
                    }).ToList(),
                    SubTotal = cart.ItemsTotal,
                    TotalAmount = cart.ItemsTotal + (cart.Tax ?? 0) - (cart.TotalDiscount ?? 0) + (cart.ShippingCost ?? 0),
                    Discount = cart.TotalDiscount,
                    Tax = cart.Tax,
                    ShippingCost = cart.ShippingCost,
                    CreatedAt = cart.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart with ID: {userId}", userId);
                throw;
            }
        }

        public async Task<decimal> GetProductTotalAsync(List<int> productIds)
        {
            try
            {
                // Retrieve cart items by product IDs
                var cartItems = await _unitOfWork.Repository.GetQueryable<CartItem>(ci => productIds.Contains(ci.ProductId))
                                        .Include(ci => ci.Product)
                                        .ToListAsync();

                // Sum and return the total prices of the retrieved cart items
                return cartItems.Sum(ci => ci.PricePerUnit);
            }
            catch (Exception ex)
            {
                // Log error if necessary
                _logger.LogError(ex, "Error occurred while calculating the total for products: {ProductIds}", string.Join(", ", productIds));
                throw;
            }
        }

        public async Task<decimal> GetTotalCartPriceAsync(int? userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null.");
            }
            // Fetch the cart for the specified user
            var cart = await _unitOfWork.Repository.GetQueryable<Cart>(c => c.UserId == userId.ToString())
                               .FirstOrDefaultAsync();

            return cart?.TotalAmount ?? 0;
        }

        public async Task<CartItem?> UpdateCartItemQuantityAsync(int cartItemId, int quantity)
        {
            // Fetch the cart item
            var cartItem = await _unitOfWork.Repository.GetQueryable<CartItem>(ci => ci.CartItemId == cartItemId)
                                       .Include(ci => ci.Cart)
                                       .ThenInclude(c => c.CartItems) // Include all cart items to recalculate totals
                                       .FirstOrDefaultAsync();
            if (cartItem == null)
            {
                return null;
            }

            // Fetch the product to check stock availability
            var product = await _unitOfWork.Repository.GetQueryable<Product>(p => p.ProductId == cartItem.ProductId)
                                     .FirstOrDefaultAsync();

            if (product == null)
            {
                return null; // Return null if the product doesn't exist
            }

            // Check if the requested quantity exceeds available stock
            if (product.StockQuantity < quantity)
            {
                return null; // Return null if there's insufficient stock
            }

            // Update the cart item’s quantity and recalculate its total price and discount
            var originalPrice = product.PricePerItem;
            var discountPercentage = product.DiscountPercentage ?? 0;
            var discountAmount = originalPrice * (discountPercentage / 100);
            var priceAfterDiscount = originalPrice - discountAmount;

            cartItem.Quantity = quantity;
            cartItem.PricePerUnit = originalPrice;
            cartItem.DiscountAmount = discountAmount * quantity;
            cartItem.TotalPrice = priceAfterDiscount * quantity;
            cartItem.UpdatedAt = DateTime.Now;

            _unitOfWork.Repository.Update(cartItem);

            // Recalculate the cart totals
            var cart = cartItem.Cart;
            cart.ItemsTotal = cart.CartItems.Sum(ci => ci.PricePerUnit * ci.Quantity);
            cart.TotalDiscount = cart.CartItems.Sum(ci => ci.DiscountAmount ?? 0);
            cart.TotalAmount = cart.CartItems.Sum(ci => ci.TotalPrice) + (cart.Tax ?? 0) + (cart.ShippingCost ?? 0);
            cart.UpdatedAt = DateTime.Now;

            _unitOfWork.Repository.Update(cart);

            await _unitOfWork.Repository.CompleteAsync();

            return cartItem; // Return the updated cart item
        }

        public async Task<CartItem?> RemoveItemFromCartAsync(int cartItemId)
        {
            // Fetch the cart item
            var cartItem = await _unitOfWork.Repository.GetQueryable<CartItem>(ci => ci.CartItemId == cartItemId)
                                        .Include(ci => ci.Cart)
                                        .ThenInclude(c => c.CartItems)
                                        .FirstOrDefaultAsync();
            if (cartItem == null)
            {
                return null;
            }

            // Get the cart associated with the cart item
            var cart = cartItem.Cart;

            // Remove the cart item from the database and the cart's collection
            _unitOfWork.Repository.HardDelete(cartItem);
            cart.CartItems.Remove(cartItem);

            // Check if the cart is now empty
            if (!cart.CartItems.Any())
            {
                // Delete the cart if it's empty
                _unitOfWork.Repository.HardDelete(cart);
            }
            else
            {
                // Recalculate the cart totals
                cart.ItemsTotal = cart.CartItems.Sum(ci => ci.PricePerUnit * ci.Quantity);
                cart.TotalDiscount = cart.CartItems.Sum(ci => ci.DiscountAmount ?? 0);
                cart.TotalAmount = (decimal)(cart.ItemsTotal - cart.TotalDiscount + (cart.Tax ?? 0) + (cart.ShippingCost ?? 0))!;
                cart.UpdatedAt = DateTime.Now;

                // Update the cart with recalculated totals
                _unitOfWork.Repository.Update(cart);
            }
            await _unitOfWork.Repository.CompleteAsync();

            return cartItem;  // Return the removed cart item
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            try
            {
                // Fetch the cart for the user
                var cart = await _unitOfWork.Repository.GetQueryable<Cart>()
                                 .Include(c => c.CartItems)
                                 .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    return false;  // Cart not found
                }

                // Remove all items from the cart
                foreach (var item in cart.CartItems.ToList())
                {
                    _unitOfWork.Repository.HardDelete(item);
                }

                // Recalculate cart totals and clear the cart
                cart.ItemsTotal = 0;
                cart.TotalAmount = 0;
                cart.CartItems.Clear();
                cart.UpdatedAt = DateTime.Now;

                // Hard delete the cart itself
                _unitOfWork.Repository.HardDelete(cart);

                // Persist changes
                await _unitOfWork.Repository.CompleteAsync();

                return true; // Cart cleared successfully
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while clearing cart for UserID: {UserId}", userId);
                return false;  // Return false in case of error
            }
        }

        public async Task<CartSummaryResponseModel> GetCartSummaryAsync(string userId)
        {
            try
            {
                // Retrieve the user's cart with the items and product details
                var cart = await _unitOfWork.Repository.GetQueryable<Cart>(c => c.UserId == userId)
                                  .Include(c => c.CartItems)
                                  .ThenInclude(ci => ci.Product)
                                  .FirstOrDefaultAsync();

                if (cart == null)
                {
                    return null!;  // Return null if no cart found
                }

                // Create the CartSummaryResponseModel from the cart data
                var cartSummary = new CartSummaryResponseModel
                {
                    Items = cart.CartItems.Select(ci => new CartItemSummary
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.ProductName,
                        Quantity = ci.Quantity,
                        PricePerUnit = ci.PricePerUnit,
                        TotalPrice = ci.TotalPrice
                    }).ToList(),
                    Subtotal = cart.ItemsTotal,
                    ShippingCost = cart.ShippingCost ?? 0,
                    Tax = cart.Tax ?? 0,
                    TotalDiscount = cart.TotalDiscount ?? 0,
                    Total = cart.TotalAmount
                };

                return cartSummary;  // Return the mapped cart summary
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving cart summary for user ID: {UserId}", userId);
                return null!;  // Return null if an error occurred
            }
        }
        public async Task<int?> GetCartItemCountAsync(string userId)
        {
            try
            {
                // Retrieve the cart including its items
                var cart = await _unitOfWork.Repository.GetQueryable<Cart>(c => c.UserId == userId)
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync();

                if (cart == null || !cart.CartItems.Any())
                {
                    return null; // Return null if no cart or items exist
                }

                // Sum up the quantity of all items in the cart
                return cart.CartItems.Sum(ci => ci.Quantity);
            }
            catch (Exception)
            {
                throw; // Let the service handle logging or exceptions
            }
        }
    }
}