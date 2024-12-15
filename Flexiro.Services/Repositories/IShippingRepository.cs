using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;

namespace Flexiro.Services.Repositories
{
    public interface IShippingRepository
    {
        Task<ShippingAddress> GetShippingAddressByUserIdAsync(string userId);
        Task<ShippingAddress> AddShippingAddressAsync(AddUpdateShippingAddressRequest request);
        Task<ShippingAddress> UpdateShippingAddressAsync(int addressId, AddUpdateShippingAddressRequest request);
        Task<IList<ShippingAddress>> GetAddressBookByUserIdAsync(string userId);
    }
}