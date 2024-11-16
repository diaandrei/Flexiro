using Flexiro.Services.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Flexiro.Services
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IShopService, ShopService>();

            return services;
        }
    }
}