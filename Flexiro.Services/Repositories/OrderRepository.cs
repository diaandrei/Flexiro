﻿using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Microsoft.EntityFrameworkCore;
using Flexiro.Application.Database;

namespace Flexiro.Services.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private readonly FlexiroDbContext _context;
        public OrderRepository(IUnitOfWork unitOfWork, IMapper mapper, FlexiroDbContext flexiroDbContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = flexiroDbContext;
        }

        public async Task<Order> CreateOrderAsync(string userId, AddUpdateShippingAddressRequest shippingAddressDto, string paymentMethod)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cart = await _unitOfWork.Repository.GetQueryable<Cart>()
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                {
                    return null!;
                }

                ShippingAddress? shippingAddress;

                var existingAddress = await _unitOfWork.Repository.GetQueryable<ShippingAddress>()
                .FirstOrDefaultAsync(addr =>
                   addr.UserId == userId &&
                   addr.Address == shippingAddressDto.Address &&
                   addr.Postcode == shippingAddressDto.Postcode &&
                   addr.City == shippingAddressDto.City &&
                   addr.Country == shippingAddressDto.Country &&
                   addr.PhoneNumber == shippingAddressDto.PhoneNumber
               );

                if (existingAddress != null)
                {
                    shippingAddress = existingAddress;
                    if (shippingAddress.AddToAddressBook != shippingAddressDto.AddToAddressBook)
                    {
                        shippingAddress.AddToAddressBook = shippingAddressDto.AddToAddressBook;

                        _unitOfWork.Repository.Update(shippingAddress);

                        await _unitOfWork.Repository.CompleteAsync();
                    }
                }
                else if (shippingAddressDto.AddToAddressBook)
                {
                    shippingAddress = _mapper.Map<ShippingAddress>(shippingAddressDto);
                    shippingAddress.UserId = userId;
                    await _unitOfWork.Repository.AddAsync(shippingAddress);
                    await _unitOfWork.Repository.CompleteAsync();
                }
                else
                {
                    shippingAddress = _mapper.Map<ShippingAddress>(shippingAddressDto);
                    shippingAddress.UserId = userId;
                    await _unitOfWork.Repository.AddAsync(shippingAddress);
                    await _unitOfWork.Repository.CompleteAsync();
                }

                decimal? shippingCost = cart.ShippingCost;
                decimal itemsTotal = cart.CartItems.Sum(item => item.Product.PricePerItem * item.Quantity - (item.DiscountAmount ?? 0));
                decimal tax = CalculateTax(itemsTotal);
                decimal totalAmount = itemsTotal + shippingCost!.Value;

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
                    PaymentMethod = paymentMethod,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository.AddAsync(order);
                await _unitOfWork.Repository.CompleteAsync();

                foreach (var item in cart.CartItems)
                {
                    item.Product.StockQuantity -= item.Quantity;
                    if (item.Product.StockQuantity <= 0)
                    {
                        item.Product.Status = ProductStatus.SoldOut;
                    }
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

                var shopIds = cart.CartItems
                    .Select(item => item.Product.ShopId)
                    .Distinct();

                foreach (var shopId in shopIds)
                {
                    var shopOwnerUserId = await _unitOfWork.Repository.GetQueryable<Shop>()
                        .Where(shop => shop.ShopId == shopId)
                        .Select(shop => shop.OwnerId)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(shopOwnerUserId))
                    {
                        var notification = new Notification
                        {
                            UserId = shopOwnerUserId,
                            Message = $"New order #{order.OrderNumber} has been received.",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false,
                            NotificationType = "Order Received"
                        };

                        await _unitOfWork.Repository.AddAsync(notification);
                    }
                }

                await _unitOfWork.Repository.CompleteAsync();

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
                    ShippingAddress = _mapper.Map<ShippingAddressResponseDto>(order.ShippingAddress),
                    OrderItems = order.OrderDetails.Select(item => new OrderItemResponseDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        PricePerUnit = item.PricePerUnit,
                        TotalPrice = (item.PricePerUnit * item.Quantity) - (item.DiscountAmount ?? 0)

                    }).ToList()
                };

                await transaction.CommitAsync();

                return order;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now.Ticks}";
        }

        public decimal CalculateShippingCost(Cart cart)
        {
            return 0.0m;
        }

        public decimal CalculateTax(decimal itemsTotal)
        {
            return 0.0m;
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
                        Postcode = o.ShippingAddress.Postcode,
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
                        Postcode = o.BillingAddress.Postcode,
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

            int deliveredOrderCount = deliveredOrders.Count;

            return (deliveredOrders, deliveredOrderCount);
        }

        public async Task<(List<string>, int)> GetAllCustomersByShopAsync(int shopId)
        {
            var customerList = await _unitOfWork.Repository.GetQueryable<Order>()
                .Where(o => o.OrderDetails.Any(od => od.ShopId == shopId))
                .Select(o => o.User.Email)
                .Distinct()
                .ToListAsync();

            int customerCount = customerList.Count;

            return (customerList, customerCount)!;
        }

        public async Task<decimal> GetTotalEarningsByShopAsync(int shopId)
        {
            var totalEarnings = await _unitOfWork.Repository.GetQueryable<OrderDetails>()
                .Where(od => od.ShopId == shopId)
                .SumAsync(od => od.TotalPrice);

            return totalEarnings;
        }

        public async Task<int> GetNewOrderCountByShopAsync(int shopId)
        {
            var newOrderCount = await _unitOfWork.Repository.GetQueryable<OrderDetails>()
                .Where(od => od.ShopId == shopId && od.Order.Status == OrderStatus.New)
                .Select(od => od.OrderId)
                .Distinct()
                .CountAsync();

            return newOrderCount;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            return (await _unitOfWork.Repository.GetQueryable<Order>()
                .FirstOrDefaultAsync(o => o.OrderId == orderId))!;
        }

        public async Task UpdateOrderAsync(Order order)
        {
            await _unitOfWork.Repository.UpdateAsync(order);
            await _unitOfWork.Repository.CompleteAsync();
        }

        public async Task<List<Order>> GetOrdersByCustomerAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.ShippingAddress)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}