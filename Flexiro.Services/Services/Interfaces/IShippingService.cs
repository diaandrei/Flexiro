using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;

namespace Flexiro.Services.Services.Interfaces
{
    public interface IShippingService
    {
        Task<ResponseModel<ShippingAddress>> GetShippingAddressByUserIdAsync(string userId);
        Task<ResponseModel<ShippingAddress>> AddShippingAddressAsync(AddUpdateShippingAddressRequest request);
        Task<ResponseModel<ShippingAddress>> UpdateShippingAddressAsync(int addressId, AddUpdateShippingAddressRequest request);
        Task<ResponseModel<IList<ShippingAddress>>> GetAddressBookByUserIdAsync(string userId);
    }
}