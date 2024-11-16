using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;

namespace Flexiro.Services.Services.Interfaces
{
    public interface IShopService
    {
        Task<ResponseModel<Shop>> CreateShopAsync(Shop createShopRequest);
        Task<ResponseModel<Shop>> UpdateShopAsync(int shopId, UpdateShopRequest updateShopRequest);
        Task<ResponseModel<Shop>> GetShopByIdAsync(int shopId);

        Task<ResponseModel<IList<Shop>>> GetActiveShopsAsync();
        Task<ResponseModel<IList<Shop>>> GetPendingShopsAsync();
        Task<ResponseModel<IList<Shop>>> GetInactiveShopsAsync();

        Task<ResponseModel<IList<Shop>>> GetAllShopsAsync();
        Task<ResponseModel<IList<Shop>>> SearchShopsByNameAsync(string shopName);
        Task<int> GetActiveShopsCountAsync();
        Task<int> GetPendingShopsCountAsync();
        Task<int> GetInactiveShopsCountAsync();

        Task<ResponseModel<Shop>> UpdateShopStatusAsync(int shopId, ShopAdminStatus newStatus);
        Task<ResponseModel<ShopResponse>> GetShopByOwnerIdAsync(string ownerId);
        Task<ResponseModel<string>> ChangeShopStatusByAdminAsync(int shopId, ShopAdminStatus newStatus);
    }
}