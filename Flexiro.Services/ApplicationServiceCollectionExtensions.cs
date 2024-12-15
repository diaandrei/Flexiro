using Flexiro.Services.Repositories;
using Flexiro.Services.Services;
using Flexiro.Services.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Flexiro.Services
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IShopService, ShopService>();
            services.AddScoped<IShippingService, ShippingService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IShippingRepository, ShippingRepository>();
            services.AddScoped<IShopRepository, ShopRepository>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IBlobStorageService, BlobStorageService>();

            return services;
        }
    }
}