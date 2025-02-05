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
            return (await _unitOfWork.Repository.GetQueryable<Cart>(c => c.UserId == userId || c.GuestUserId == userId)
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync())!;
        }

        public async Task<Cart> CreateNewCartAsync(string userId, bool IsGuest)
        {
            var cart = new Cart
            {
                UserId = IsGuest ? null! : userId,
                UserId = IsGuest ? null : userId, // Set UserId to null if it's a guest user
                GuestUserId = IsGuest ? userId : null,
                CartItems = new List<CartItem>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository.AddAsync(cart);
            return cart;
        }

        public async Task<CartItem> AddOrUpdateCartItemAsync(Cart cart, CartItemRequestModel itemRequest, Product product)
        {
            var finalPrice = product.DiscountPercentage.HasValue && product.DiscountPercentage.Value != 0
                ? product.PricePerItem - (product.PricePerItem * (product.DiscountPercentage.Value / 100))
                : product.PricePerItem;

            var originalPrice = finalPrice;
            decimal discountAmount = 0;
            decimal priceAfterDiscount = originalPrice;

            if (product.DiscountPercentage.HasValue && product.DiscountPercentage > 0)
            {
                discountAmount = originalPrice * product.DiscountPercentage.Value / 100;
                priceAfterDiscount = originalPrice - discountAmount;
            }

            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == itemRequest.ProductId && ci.ShopId == itemRequest.ShopId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity = itemRequest.Quantity;
                existingCartItem.PricePerUnit = originalPrice;
                existingCartItem.DiscountAmount = discountAmount * itemRequest.Quantity;
                existingCartItem.TotalPrice = priceAfterDiscount * itemRequest.Quantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository.AddAsync(newCartItem);
                return newCartItem;
            }
        }

        public async Task UpdateCartTotalsAsync(Cart cart)
        {
            cart.ItemsTotal = cart.CartItems.Sum(ci => ci.PricePerUnit * ci.Quantity);
            cart.ShippingCost = 5;
            cart.TotalDiscount = cart.CartItems.Sum(ci => ci.DiscountAmount ?? 0);
            cart.TotalAmount = cart.CartItems.Sum(ci => ci.TotalPrice) + (cart.Tax ?? 0) + (cart.ShippingCost ?? 0);
            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Repository.CompleteAsync();
        }

        public async Task<bool> IsCartExistAsync(int? userId)
        {
            var cart = await _unitOfWork.Repository.GetQueryable<Cart>(c => c.UserId == userId.ToString())
                            .Include(c => c.CartItems)
                            .FirstOrDefaultAsync();

            return cart != null && cart.CartItems.Any();
        }

        public async Task<MainCartModel> GetCartAsync(string userId)
        {
            try
            {
                var cart = await _unitOfWork.Repository.GetQueryable<Cart>(c => c.UserId == userId || c.GuestUserId == userId)
                                .Include(c => c.CartItems)
                                    .ThenInclude(ci => ci.Product)
                                    .ThenInclude(p => p.ProductImages)
                                .FirstOrDefaultAsync();

                if (cart == null) return null!;

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
                var cartItems = await _unitOfWork.Repository.GetQueryable<CartItem>(ci => productIds.Contains(ci.ProductId))
                                        .Include(ci => ci.Product)
                                        .ToListAsync();

                return cartItems.Sum(ci => ci.PricePerUnit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calculating product total for products: {ProductIds}", string.Join(", ", productIds));
                throw;
            }
        }

        public async Task<decimal> GetTotalCartPriceAsync(int? userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null.");
            }

            var cart = await _unitOfWork.Repository.GetQueryable<Cart>(c => c.UserId == userId.ToString())
                               .FirstOrDefaultAsync();

            return cart?.TotalAmount ?? 0;

        }

        public async Task<CartItem?> UpdateCartItemQuantityAsync(int cartItemId, int quantity)
        {
            var cartItem = await _unitOfWork.Repository.GetQueryable<CartItem>(ci => ci.CartItemId == cartItemId)
                                       .Include(ci => ci.Cart)
                                       .ThenInclude(c => c.CartItems)
                                       .FirstOrDefaultAsync();

            if (cartItem == null)
            {
                return null;
            }

            var product = await _unitOfWork.Repository.GetQueryable<Product>(p => p.ProductId == cartItem.ProductId)
                                     .FirstOrDefaultAsync();

            if (product == null)
            {
                return null;
            }

            if (product.StockQuantity < quantity)
            {
                return null;
            }

            var originalPrice = product.PricePerItem;
            var discountPercentage = product.DiscountPercentage ?? 0;
            var discountAmount = originalPrice * (discountPercentage / 100);
            var priceAfterDiscount = originalPrice - discountAmount;

            cartItem.Quantity = quantity;
            cartItem.PricePerUnit = originalPrice;
            cartItem.DiscountAmount = discountAmount * quantity;
            cartItem.TotalPrice = priceAfterDiscount * quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository.Update(cartItem);

            var cart = cartItem.Cart;
            cart.ItemsTotal = cart.CartItems.Sum(ci => ci.PricePerUnit * ci.Quantity);
            cart.TotalDiscount = cart.CartItems.Sum(ci => ci.DiscountAmount ?? 0);
            cart.TotalAmount = cart.CartItems.Sum(ci => ci.TotalPrice) + (cart.Tax ?? 0) + (cart.ShippingCost ?? 0);
            cart.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository.Update(cart);

            await _unitOfWork.Repository.CompleteAsync();

            return cartItem;
        }

        public async Task<CartItem?> RemoveItemFromCartAsync(int cartItemId)
        {
            var cartItem = await _unitOfWork.Repository.GetQueryable<CartItem>(ci => ci.CartItemId == cartItemId)
                                        .Include(ci => ci.Cart)
                                        .ThenInclude(c => c.CartItems)
                                        .FirstOrDefaultAsync();

            if (cartItem == null)
            {
                return null;
            }

            var cart = cartItem.Cart;

            _unitOfWork.Repository.HardDelete(cartItem);
            cart.CartItems.Remove(cartItem);

            if (!cart.CartItems.Any())
            {
                _unitOfWork.Repository.HardDelete(cart);
            }
            else
            {
                cart.ItemsTotal = cart.CartItems.Sum(ci => ci.PricePerUnit * ci.Quantity);
                cart.TotalDiscount = cart.CartItems.Sum(ci => ci.DiscountAmount ?? 0);
                cart.TotalAmount = (decimal)(cart.ItemsTotal - cart.TotalDiscount + (cart.Tax ?? 0) + (cart.ShippingCost ?? 0));
                cart.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository.Update(cart);
            }

            await _unitOfWork.Repository.CompleteAsync();

            if (cart != null!)
            {
                cartItem.Cart = cart;

            }
            return cartItem;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            try
            {
                var cart = await _unitOfWork.Repository.GetQueryable<Cart>()
                                 .Include(c => c.CartItems)
                                 .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    return false;
                }

                foreach (var item in cart.CartItems.ToList())
                {
                    _unitOfWork.Repository.HardDelete(item);
                }

                cart.ItemsTotal = 0;
                cart.TotalAmount = 0;
                cart.CartItems.Clear();
                cart.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository.HardDelete(cart);

                await _unitOfWork.Repository.CompleteAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while clearing cart for UserID: {UserId}", userId);
                return false;
            }
        }

        public async Task<CartSummaryResponseModel> GetCartSummaryAsync(string userId)
        {
            try
            {
                var cart = await _unitOfWork.Repository.GetQueryable<Cart>(c => c.UserId == userId)
                                  .Include(c => c.CartItems)
                                  .ThenInclude(ci => ci.Product)
                                  .FirstOrDefaultAsync();

                if (cart == null)
                {
                    return null!;
                }

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

                return cartSummary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving cart summary for user ID: {UserId}", userId);
                return null!;
            }
        }

        public async Task<int?> GetCartItemCountAsync(string userId)
        {
            try
            {
                var cart = await _unitOfWork.Repository.GetQueryable<Cart>(c => c.UserId == userId || c.GuestUserId == userId)
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync();

                if (cart == null || !cart.CartItems.Any())
                {
                    return null;
                }

                return cart.CartItems.Sum(ci => ci.Quantity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving the cart item count for user ID: {userId}", ex);
            }
        }

        public async Task UpdateCartAsync(Cart cart)
        {
            _unitOfWork.Repository.Update(cart);
            await _unitOfWork.Repository.CompleteAsync();
        }

    }
}
