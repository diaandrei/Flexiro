using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services.Interfaces;

namespace Flexiro.Services.Services
{
    public class ShippingService : IShippingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IShippingRepository _shippingRepository;

        public ShippingService(IUnitOfWork unitOfWork, IMapper mapper, IShippingRepository shippingRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _shippingRepository = shippingRepository;
        }

        public async Task<ResponseModel<ShippingAddress>> GetShippingAddressByUserIdAsync(string userId)
        {
            var response = new ResponseModel<ShippingAddress>();

            try
            {
                // Retrieve the address from the repository
                var address = await _shippingRepository.GetShippingAddressByUserIdAsync(userId);

                // Check if the address exists
                if (address == null!)
                {
                    response.Success = false;
                    response.Title = "Address Not Found";
                    response.Description = $"Default shipping address for user ID '{userId}' does not exist.";
                    return response;
                }

                // Set successful response
                response.Success = true;
                response.Content = address;
                response.Title = "Address Retrieved Successfully";
                response.Description = "Default shipping address retrieved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Address";
                response.Description = "An error occurred while retrieving the address.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<ShippingAddress>> AddShippingAddressAsync(AddUpdateShippingAddressRequest request)
        {
            var response = new ResponseModel<ShippingAddress>();

            try
            {
                // Add the new address using the repository method
                var newAddress = await _shippingRepository.AddShippingAddressAsync(request);

                // Set successful response
                response.Success = true;
                response.Content = newAddress;
                response.Title = "Address Added Successfully";
                response.Description = "Shipping address added to the address book.";
            }
            catch (Exception ex)
            {
                // Handle any exceptions and set failure response
                response.Success = false;
                response.Title = "Error Adding Address";
                response.Description = "An error occurred while adding the address.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<ShippingAddress>> UpdateShippingAddressAsync(int addressId, AddUpdateShippingAddressRequest request)
        {
            var response = new ResponseModel<ShippingAddress>();

            try
            {
                // Attempt to update the address using the repository method
                var updatedAddress = await _shippingRepository.UpdateShippingAddressAsync(addressId, request);

                if (updatedAddress == null!)
                {
                    // Address not found case
                    response.Success = false;
                    response.Title = "Address Not Found";
                    response.Description = $"Address with ID '{addressId}' does not exist.";
                    return response;
                }

                // Success response with the updated address
                response.Success = true;
                response.Content = updatedAddress;
                response.Title = "Address Updated Successfully";
                response.Description = "Shipping address updated.";
            }
            catch (Exception ex)
            {
                // Exception handling
                response.Success = false;
                response.Title = "Error Updating Address";
                response.Description = "An error occurred while updating the address.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<IList<ShippingAddress>>> GetAddressBookByUserIdAsync(string userId)
        {
            var response = new ResponseModel<IList<ShippingAddress>>();

            try
            {
                var addresses = await _shippingRepository.GetAddressBookByUserIdAsync(userId);

                response.Success = true;
                response.Content = addresses;
                response.Title = "Address Book Retrieved Successfully";
                response.Description = "All shipping addresses for the user have been retrieved.";
            }
            catch (Exception ex)
            {
                // Exception handling
                response.Success = false;
                response.Title = "Error Retrieving Address Book";
                response.Description = "An error occurred while retrieving the address book.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }
    }
}