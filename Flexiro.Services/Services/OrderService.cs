using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services.Interfaces;

namespace Flexiro.Services.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IShippingRepository _shippingAddressRepository;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IShippingRepository shippingAddressRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _shippingAddressRepository = shippingAddressRepository;
        }

        public async Task<ResponseModel<OrderResponseDto>> PlaceOrderAsync(string userId, AddUpdateShippingAddressRequest shippingAddressDto, string paymentMethod)
        {
            var response = new ResponseModel<OrderResponseDto>();

            try
            {
                // Call the repository method to handle data fetching and calculations
                var order = await _orderRepository.CreateOrderAsync(userId, shippingAddressDto, paymentMethod);

                if (order == null!)
                {
                    response.Success = false;
                    response.Title = "Cart is empty";
                    return response;
                }

                // Prepare OrderResponseDto for the user
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

        public async Task<int> GetTotalOrdersByShopAsync(int shopId)
        {
            var totalOrders = await _orderRepository.GetTotalOrdersByShopAsync(shopId);
            return totalOrders;
        }

        public async Task<GroupedOrdersDto> GetGroupedOrdersByShopAsync(int shopId)
        {
            var allOrders = await _orderRepository.GetOrdersByShopAsync(shopId);

            var groupedOrders = new GroupedOrdersDto
            {
                NewOrders = allOrders.Where(o => o.Status == OrderStatus.New).ToList(),
                PendingOrders = allOrders.Where(o => o.Status == OrderStatus.Pending).ToList(),
                ProcessingOrders = allOrders.Where(o => o.Status == OrderStatus.Processing).ToList(),
                ShippedOrders = allOrders.Where(o => o.Status == OrderStatus.Shipped).ToList(),
                DeliveredOrders = allOrders.Where(o => o.Status == OrderStatus.Delivered).ToList(),
                CanceledOrders = allOrders.Where(o => o.Status == OrderStatus.Canceled).ToList(),
                ReturnedOrders = allOrders.Where(o => o.Status == OrderStatus.Returned).ToList(),
                CompletedOrders = allOrders.Where(o => o.Status == OrderStatus.Completed).ToList(),
                AllOrders = allOrders
            };

            return groupedOrders;
        }

        public async Task<(List<Order>, int)> GetDeliveredOrdersByShopAsync(int shopId)
        {
            return await _orderRepository.GetDeliveredOrdersByShopAsync(shopId);
        }

        public async Task<(List<string>, int)> GetAllCustomersByShopAsync(int shopId)
        {
            // Get the list of unique customers for the specified shopId
            return await _orderRepository.GetAllCustomersByShopAsync(shopId);
        }
        public async Task<decimal> GetTotalEarningsByShopAsync(int shopId)
        {
            // Get the total earnings for the specified shopId from delivered orders
            var totalEarnings = await _orderRepository.GetTotalEarningsByShopAsync(shopId);
            return totalEarnings;
        }
        public async Task<int> GetNewOrderCountByShopAsync(int shopId)
        {
            // Get the count of new orders for the specified shopId
            var newOrderCount = await _orderRepository.GetNewOrderCountByShopAsync(shopId);
            return newOrderCount;
        }

        public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto request)
        {
            var order = await _orderRepository.GetOrderByIdAsync(request.OrderId);

            if (order == null!)
                return false;

            order.Status = (OrderStatus)request.NewStatus;
            await _orderRepository.UpdateOrderAsync(order);
            return true;
        }

        public async Task<List<CustomerOrderResponseDto>> GetOrdersByCustomerAsync(string userId)
        {
            var orders = await _orderRepository.GetOrdersByCustomerAsync(userId);

            if (orders == null! || !orders.Any())
                return new List<CustomerOrderResponseDto>();

            return orders.Select(order => new CustomerOrderResponseDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                ItemsTotal = order.ItemsTotal,
                ShippingCost = order.ShippingCost,
                Tax = order.Tax,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                PaymentStatus = order.PaymentStatus!,
                PaymentMethod = order.PaymentMethod!,
                CreatedAt = order.CreatedAt,
                ShippingAddress = new ShippingAddressResponseDto
                {
                    Address = order.ShippingAddress.Address,
                    City = order.ShippingAddress.City,
                    Postcode = order.ShippingAddress.Postcode
                },
                OrderItems = order.OrderDetails.Select(item => new CustomerOrderItemResponseDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.ProductName,
                    Quantity = item.Quantity,
                    PricePerUnit = item.PricePerUnit,
                    DiscountAmount = item.DiscountAmount ?? 0,
                    TotalPrice = item.TotalPrice
                }).ToList()
            }).ToList();
        }
    }
}