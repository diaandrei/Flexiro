using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;

namespace Flexiro.Services.Repositories
{
    public interface IShopRepository
    {
        Task<Shop> CreateShopAsync(Shop createShopRequest);
        Task<Shop> UpdateShopAsync(UpdateShopRequest updateShopRequest);
        Task<Shop> GetShopByIdAsync(int shopId);
        Task<IList<Shop>> GetActiveShopsAsync();
        Task<IList<Shop>> GetPendingShopsAsync();
        Task<IList<Shop>> GetInactiveShopsAsync();

        Task<IList<Shop>> GetAllShopsAsync();
        Task<IList<Shop>> SearchShopsByNameAsync(string shopName);
        Task<int> GetActiveShopsCountAsync();
        Task<int> GetPendingShopsCountAsync();
        Task<int> GetInactiveShopsCountAsync();

        Task<Shop> UpdateShopStatusAsync(int shopId, ShopAdminStatus newStatus);
        Task<ShopResponse> GetShopByOwnerIdAsync(string ownerId);
        Task<bool> ChangeShopStatusAsync(int shopId, int newStatus);
        Task<bool> UpdateShopSellerStatusAsync(ShopStatus newStatus);
    }
}