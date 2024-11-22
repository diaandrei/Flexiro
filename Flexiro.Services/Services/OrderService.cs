using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Flexiro.Services.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResponseModel<OrderResponseDto>> PlaceOrderAsync(string userId, AddUpdateShippingAddressRequest shippingAddressDto)
        {
            var response = new ResponseModel<OrderResponseDto>();

            try
            {
                var cart = await _unitOfWork.Repository.GetQueryable<Cart>()
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                {
                    response.Success = false;
                    response.Title = "Cart is empty";
                    return response;
                }

                ShippingAddress shippingAddress;

                if (shippingAddressDto.AddToAddressBook)
                {
                    shippingAddress = _mapper.Map<ShippingAddress>(shippingAddressDto);
                    shippingAddress.UserId = userId;
                    await _unitOfWork.Repository.AddAsync(shippingAddress);
                    await _unitOfWork.Repository.CompleteAsync();
                }
                else
                {
                    // Use an existing address if available
                    shippingAddress = (await _unitOfWork.Repository.GetQueryable<ShippingAddress>()
                        .FirstOrDefaultAsync(addr => addr.UserId == userId &&
                                                     addr.Address == shippingAddressDto.Address &&
                                                     addr.ZipCode == shippingAddressDto.ZipCode))!;
                    if (shippingAddress == null!)
                    {
                        shippingAddress = _mapper.Map<ShippingAddress>(shippingAddressDto);
                        shippingAddress.UserId = userId;
                        await _unitOfWork.Repository.AddAsync(shippingAddress);
                        await _unitOfWork.Repository.CompleteAsync();
                    }
                }

                // Calculate totals
                decimal itemsTotal = cart.CartItems.Sum(item => item.Product.PricePerItem * item.Quantity - (item.DiscountAmount ?? 0));
                decimal shippingCost = CalculateShippingCost(cart);
                decimal tax = CalculateTax(itemsTotal);
                decimal totalAmount = itemsTotal + shippingCost + tax;

                // Create and save Order
                var order = new Order
                {
                    UserId = userId,
                    ShippingAddressId = shippingAddress.ShippingAddressId,
                    BillingAddressId = shippingAddress.ShippingAddressId,
                    OrderNumber = GenerateOrderNumber(),
                    ItemsTotal = itemsTotal,
                    ShippingCost = shippingCost,
                    Tax = tax,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.New,
                    PaymentStatus = "Unpaid",
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository.AddAsync(order);
                await _unitOfWork.Repository.CompleteAsync();

                // Create and save OrderDetails for each cart item
                foreach (var item in cart.CartItems)
                {
                    var orderDetail = new OrderDetails
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        ShopId = item.Product.ShopId,
                        Quantity = item.Quantity,
                        PricePerUnit = item.Product.PricePerItem,
                        DiscountAmount = item.DiscountAmount,
                        TotalPrice = (item.Product.PricePerItem * item.Quantity) - (item.DiscountAmount ?? 0),
                        Status = OrderStatus.New,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Repository.AddAsync(orderDetail);
                }

                await _unitOfWork.Repository.CompleteAsync();

                // Create and prepare the OrderResponseDto to be returned to the user
                var orderResponse = new OrderResponseDto
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    ItemsTotal = order.ItemsTotal,
                    ShippingCost = order.ShippingCost,
                    Tax = order.Tax,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    CreatedAt = order.CreatedAt,
                    ShippingAddress = _mapper.Map<ShippingAddressResponseDto>(shippingAddress),
                    OrderItems = cart.CartItems.Select(item => new OrderItemResponseDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        PricePerUnit = item.Product.PricePerItem,
                        TotalPrice = (item.Product.PricePerItem * item.Quantity) - (item.DiscountAmount ?? 0)
                    }).ToList()
                };

                response.Success = true;
                response.Content = orderResponse;
                response.Title = "Order Placed Successfully";
                response.Description = "Your order has been placed successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Order Placement Failed";
                response.Description = "An error occurred while placing the order.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow.Ticks}";
        }

        public decimal CalculateShippingCost(Cart cart)
        {
            return 10.0m;
        }

        public decimal CalculateTax(decimal itemsTotal)
        {
            return itemsTotal * 0.05m;
        }

        public async Task<int> GetTotalOrdersByShopAsync(int shopId)
        {
            var totalOrders = await _unitOfWork.Repository.GetQueryable<OrderDetails>()
                .Where(od => od.ShopId == shopId)
                .Select(od => od.OrderId)
                .Distinct()
                .CountAsync();
            return totalOrders;
        }
        public async Task<List<OrderDetailDto>> GetOrdersByShopAsync(int shopId)
        {
            var orders = await _unitOfWork.Repository.GetQueryable<Order>()
                .Where(o => o.OrderDetails.Any(od => od.ShopId == shopId))
                .Select(o => new OrderDetailDto
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    UserId = o.UserId,
                    ItemsTotal = o.ItemsTotal,
                    ShippingCost = o.ShippingCost ?? 0,
                    Tax = o.Tax ?? 0,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus!,
                    PaymentMethod = o.PaymentMethod!,
                    CreatedAt = o.CreatedAt,
                    DeliveryDate = o.DeliveryDate,
                    ShippingAddress = new ShippingAddressDto
                    {
                        FirstName = o.ShippingAddress.FirstName,
                        LastName = o.ShippingAddress.LastName,
                        Email = o.ShippingAddress.Email,
                        Address = o.ShippingAddress.Address,
                        City = o.ShippingAddress.City,
                        State = o.ShippingAddress.State,
                        ZipCode = o.ShippingAddress.ZipCode,
                        Country = o.ShippingAddress.Country,
                        PhoneNumber = o.ShippingAddress.PhoneNumber,
                        Note = o.ShippingAddress.Note
                    },
                    BillingAddress = new BillingAddressDto
                    {
                        FirstName = o.BillingAddress.FirstName,
                        LastName = o.BillingAddress.LastName,
                        Email = o.BillingAddress.Email,
                        Address = o.BillingAddress.Address,
                        City = o.BillingAddress.City,
                        State = o.BillingAddress.State,
                        ZipCode = o.BillingAddress.ZipCode,
                        Country = o.BillingAddress.Country,
                        PhoneNumber = o.BillingAddress.PhoneNumber,
                        Note = o.BillingAddress.Note
                    },
                    OrderItems = o.OrderDetails
                        .Where(od => od.ShopId == shopId)
                        .Select(od => new OrderItemDto
                        {
                            OrderDetailsId = od.OrderDetailsId,
                            ProductId = od.ProductId,
                            ProductName = od.Product.ProductName,
                            Quantity = od.Quantity,
                            PricePerUnit = od.PricePerUnit,
                            DiscountAmount = od.DiscountAmount ?? 0,
                            TotalPrice = od.TotalPrice,
                            Status = od.Status,
                            CreatedAt = od.CreatedAt
                        }).ToList()
                })
                .ToListAsync();

            return orders;
        }

        public async Task<(List<Order>, int)> GetDeliveredOrdersByShopAsync(int shopId)
        {
            var deliveredOrders = await _unitOfWork.Repository.GetQueryable<Order>()
                .Where(o => o.OrderDetails.Any(od => od.ShopId == shopId && od.Status == OrderStatus.Delivered))
                .ToListAsync();

            // Count of delivered orders
            int deliveredOrderCount = deliveredOrders.Count;

            return (deliveredOrders, deliveredOrderCount);
        }

        public async Task<(List<string>, int)> GetAllCustomersByShopAsync(int shopId)
        {
            // Get the list of unique customers for the specified shopId
            var customerList = await _unitOfWork.Repository.GetQueryable<Order>()
                .Where(o => o.OrderDetails.Any(od => od.ShopId == shopId))
                .Select(o => o.User.Email)
                .Distinct()
                .ToListAsync();

            // Count of unique customers
            int customerCount = customerList.Count;

            return (customerList, customerCount)!;
        }
        public async Task<decimal> GetTotalEarningsByShopAsync(int shopId)
        {
            // Get the total earnings for the specified shopId from delivered orders
            var totalEarnings = await _unitOfWork.Repository.GetQueryable<OrderDetails>()
                .Where(od => od.ShopId == shopId && od.Order.Status == OrderStatus.Delivered)
                .SumAsync(od => od.TotalPrice);

            return totalEarnings;
        }
        public async Task<int> GetNewOrderCountByShopAsync(int shopId)
        {
            // Get the count of new orders for the specified shopId
            var newOrderCount = await _unitOfWork.Repository.GetQueryable<OrderDetails>()
                .Where(od => od.ShopId == shopId && od.Order.Status == OrderStatus.New)
                .Select(od => od.OrderId)
                .Distinct()
                .CountAsync();

            return newOrderCount;
        }
    }
}