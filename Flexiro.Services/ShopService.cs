using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Repositories;
using Flexiro.Services.Services.Interfaces;

namespace Flexiro.Services
{
    public class ShopService : IShopService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IShopRepository _shopRepository;
        private readonly IBlobStorageService _blobStorageService;
        public ShopService(IUnitOfWork unitOfWork, IMapper mapper, IShopRepository shopRepository, IBlobStorageService blobStorageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _shopRepository = shopRepository;
            _blobStorageService = blobStorageService;
        }
        public async Task<ResponseModel<Shop>> CreateShopAsync(Shop createShopRequest)
        {
            var response = new ResponseModel<Shop>();

            try
            {
                // Call the repository method to create a new shop
                var newShop = await _shopRepository.CreateShopAsync(createShopRequest);

                // Set successful response with the created shop details
                response.Success = true;
                response.Content = newShop;
                response.Title = "Shop Created Successfully";
                response.Description = $"Shop '{newShop.ShopName}' has been created.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Creating Shop";
                response.Description = "An error occurred while creating the shop.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<Shop>> UpdateShopAsync(UpdateShopRequest updateShopRequest)
        {
            var response = new ResponseModel<Shop>();

            try
            {
                if (updateShopRequest.ShopLogo != null)
                {
                    // Upload the new logo to Blob Storage
                    await using (var stream = updateShopRequest.ShopLogo.OpenReadStream())
                    {
                        var imageUrl = await _blobStorageService.UploadImageAsync(stream, updateShopRequest.ShopLogo.FileName);
                        updateShopRequest.NewLogoPath = imageUrl; // Save relative path or file name
                    }
                }

                var updatedShop = await _shopRepository.UpdateShopAsync(updateShopRequest);

                response.Success = true;
                response.Content = updatedShop;
                response.Title = "Shop Updated Successfully";
                response.Description = $"Shop '{updatedShop.ShopName}' has been updated.";
            }
            catch (KeyNotFoundException ex)
            {
                // Handle the case where the shop was not found
                response.Success = false;
                response.Title = "Shop Not Found";
                response.Description = ex.Message;
            }
            catch (Exception ex)
            {
                // Handle general exceptions and set failure response
                response.Success = false;
                response.Title = "Error Updating Shop";
                response.Description = "An error occurred while updating the shop.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<Shop>> GetShopByIdAsync(int shopId)
        {
            var response = new ResponseModel<Shop>();

            try
            {
                // Call the repository to retrieve the shop by ID
                var shop = await _shopRepository.GetShopByIdAsync(shopId);

                // Check if the shop exists
                if (shop == null!)
                {
                    response.Success = false;
                    response.Title = "Shop Not Found";
                    response.Description = $"Shop with ID '{shopId}' does not exist.";
                    return response;
                }

                // Set successful response
                response.Success = true;
                response.Content = shop;
                response.Title = "Shop Retrieved Successfully";
                response.Description = $"Shop '{shop.ShopName}' has been retrieved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Shop";
                response.Description = "An error occurred while retrieving the shop.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<IList<Shop>>> GetActiveShopsAsync()
        {
            var response = new ResponseModel<IList<Shop>>();

            try
            {
                // Retrieve active shops from the repository
                var activeShops = await _shopRepository.GetActiveShopsAsync();

                // Set successful response
                response.Success = true;
                response.Content = activeShops;
                response.Title = "Active Shops Retrieved Successfully";
                response.Description = "All active shops have been retrieved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Active Shops";
                response.Description = "An error occurred while retrieving active shops.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<IList<Shop>>> GetPendingShopsAsync()
        {
            var response = new ResponseModel<IList<Shop>>();

            try
            {
                // Retrieve pending shops from the repository
                var pendingShops = await _shopRepository.GetPendingShopsAsync();

                // Set successful response
                response.Success = true;
                response.Content = pendingShops;
                response.Title = "Pending Shops Retrieved Successfully";
                response.Description = "All pending shops have been retrieved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Pending Shops";
                response.Description = "An error occurred while retrieving pending shops.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<IList<Shop>>> GetInactiveShopsAsync()
        {
            var response = new ResponseModel<IList<Shop>>();

            try
            {
                // Retrieve inactive shops from the repository
                var inactiveShops = await _shopRepository.GetInactiveShopsAsync();

                // Set successful response
                response.Success = true;
                response.Content = inactiveShops;
                response.Title = "Inactive Shops Retrieved Successfully";
                response.Description = "All inactive shops have been retrieved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Inactive Shops";
                response.Description = "An error occurred while retrieving inactive shops.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<IList<Shop>>> GetAllShopsAsync()
        {
            var response = new ResponseModel<IList<Shop>>();

            try
            {
                // Retrieve all shops from the repository
                var allShops = await _shopRepository.GetAllShopsAsync();

                // Set successful response
                response.Success = true;
                response.Content = allShops;
                response.Title = "All Shops Retrieved Successfully";
                response.Description = "All shops have been retrieved successfully.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving All Shops";
                response.Description = "An error occurred while retrieving all shops.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<IList<Shop>>> SearchShopsByNameAsync(string shopName)
        {
            var response = new ResponseModel<IList<Shop>>();

            try
            {
                // Retrieve shops that match the given name from the repository
                var shops = await _shopRepository.SearchShopsByNameAsync(shopName);

                // Set successful response
                response.Success = true;
                response.Content = shops;
                response.Title = "Shops Retrieved Successfully";
                response.Description = $"Shops containing '{shopName}' have been retrieved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Searching Shops";
                response.Description = "An error occurred while searching for shops.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<int> GetActiveShopsCountAsync()
        {
            var activeShopsCount = await _shopRepository.GetActiveShopsCountAsync();
            return activeShopsCount;
        }

        public async Task<int> GetPendingShopsCountAsync()
        {
            var pendingShopsCount = await _shopRepository.GetPendingShopsCountAsync();
            return pendingShopsCount;
        }
        public async Task<int> GetInactiveShopsCountAsync()
        {
            var inactiveShopsCount = await _shopRepository.GetInactiveShopsCountAsync();
            return inactiveShopsCount;
        }
        public async Task<ResponseModel<Shop>> UpdateShopStatusAsync(int shopId, ShopAdminStatus newStatus)
        {
            var response = new ResponseModel<Shop>();

            try
            {
                // Attempt to update the shop status using the repository method
                var updatedShop = await _shopRepository.UpdateShopStatusAsync(shopId, newStatus);

                // Check if the shop was found and updated
                if (updatedShop == null!)
                {
                    response.Success = false;
                    response.Title = "Shop Not Found";
                    response.Description = $"Shop with ID '{shopId}' does not exist.";
                    return response;
                }

                // Set successful response
                response.Success = true;
                response.Content = updatedShop;
                response.Title = "Shop Status Updated Successfully";
                response.Description = $"Shop '{updatedShop.ShopName}' status has been updated to '{newStatus}'.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Updating Shop Status";
                response.Description = "An error occurred while updating the shop status.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<ShopResponse>> GetShopByOwnerIdAsync(string ownerId)
        {
            var response = new ResponseModel<ShopResponse>();

            try
            {
                // Attempt to retrieve the shop using the repository method
                var shop = await _shopRepository.GetShopByOwnerIdAsync(ownerId);

                // Check if the shop exists
                if (shop == null!)
                {
                    response.Success = false;
                    response.Title = "Shop Not Found";
                    response.Description = $"Shop with owner ID '{ownerId}' does not exist.";
                    return response;
                }

                // Map the shop entity to the response model
                var shopResponse = _mapper.Map<ShopResponse>(shop);

                // Set successful response
                response.Success = true;
                response.Content = shopResponse;
                response.Title = "Shop Retrieved Successfully";
                response.Description = $"Shop owned by '{ownerId}' has been retrieved.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Retrieving Shop";
                response.Description = "An error occurred while retrieving the shop by owner ID.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<string>> ChangeShopStatusByAdminAsync(int shopId, int newstatus)
        {
            var response = new ResponseModel<string>();

            try
            {
                // Attempt to update the shop status using the repository method
                var updateSuccess = await _shopRepository.ChangeShopStatusAsync(shopId, newstatus);

                if (!updateSuccess)
                {
                    response.Success = false;
                    response.Title = "Shop Not Found";
                    response.Description = $"No shop found with ID {shopId}.";
                    return response;
                }

                // Set successful response
                response.Success = true;
                response.Title = "Shop Status Updated";
                response.Description = $"Shop status successfully updated to {newstatus}.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
                response.Success = false;
                response.Title = "Error Updating Shop Status";
                response.Description = "An error occurred while updating the shop status.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel<string>> ChangeShopSellerStatusAsync(ShopStatus newStatus)
        {
            var response = new ResponseModel<string>();

            try
            {
                // Update the shop's seller status
                var isUpdated = await _shopRepository.UpdateShopSellerStatusAsync(newStatus);

                if (!isUpdated)
                {
                    response.Success = false;
                    response.Title = "Shop Not Found";
                    response.Description = $"No shop with ID '{newStatus.ShopId}' was found.";
                    return response;
                }

                response.Success = true;
                response.Title = "Shop Status Updated Successfully";
                response.Description = $"The shop with ID '{newStatus.ShopId}' is now '{newStatus}'.";
            }
            catch (Exception ex)
            {
                response.Success = false;

                response.Title = "Error Updating Shop Status";
                response.Description = "An error occurred while updating the shop's seller status.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }
    }
}