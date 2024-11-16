using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Flexiro.Services
{
    public class ShopService : IShopService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShopService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ResponseModel<Shop>> CreateShopAsync(Shop createShopRequest)
        {
            var response = new ResponseModel<Shop>();

            try
            {
                // Map the incoming create request to a new Shop entity using AutoMapper
                var newShop = _mapper.Map<Shop>(createShopRequest);

                // Add the new shop to the repository
                await _unitOfWork.Repository.AddAsync(newShop);
                await _unitOfWork.Repository.CompleteAsync();

                // Set successful response
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
        public async Task<ResponseModel<Shop>> UpdateShopAsync(int shopId, UpdateShopRequest updateShopRequest)
        {
            var response = new ResponseModel<Shop>();

            try
            {
                // Retrieve the existing shop from the repository
                var existingShop = await _unitOfWork.Repository
                    .GetQueryable<Shop>(s => s.ShopId == shopId)
                    .FirstOrDefaultAsync();

                // Check if the shop exists
                if (existingShop == null)
                {
                    response.Success = false;
                    response.Title = "Shop Not Found";

                    response.Description = $"Shop with ID '{shopId}' does not exist.";
                    return response;
                }

                if (updateShopRequest.ShopLogo != null!)
                {
                    // Generate a new file name and path for the new logo
                    var newFileName = $"{Guid.NewGuid()}_{updateShopRequest.ShopLogo.FileName}";
                    var newFilePath = Path.Combine("wwwroot/uploads/logos", newFileName);

                    // Save the new file to the server
                    await using (var stream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await updateShopRequest.ShopLogo.CopyToAsync(stream);
                    }

                    // Optionally delete the old logo file if it exists
                    if (!string.IsNullOrEmpty(existingShop.ShopLogo))
                    {
                        var oldFilePath = Path.Combine("wwwroot", existingShop.ShopLogo.TrimStart('/'));

                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                    }

                    // Update the logo path in the database
                    existingShop.ShopLogo = $"/uploads/logos/{newFileName}";
                }
                // Map the updated fields from the request to the existing shop entity
                var responsep = _mapper.Map(updateShopRequest, existingShop);

                // Update shop in the repository
                _unitOfWork.Repository.Update(existingShop);
                await _unitOfWork.Repository.CompleteAsync();

                // Set successful response
                response.Success = true;
                response.Content = responsep;
                response.Title = "Shop Updated Successfully";
                response.Description = $"Shop '{existingShop.ShopName}' has been updated.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set failure response
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
                var shop = await _unitOfWork.Repository
                    .GetQueryable<Shop>(s => s.ShopId == shopId)
                    .FirstOrDefaultAsync();

                // Check if the shop exists
                if (shop == null)
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
                var activeShops = await _unitOfWork.Repository
                    .GetQueryable<Shop>(s => s.AdminStatus == ShopAdminStatus.Active)
                    .ToListAsync();

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
                var pendingShops = await _unitOfWork.Repository
                    .GetQueryable<Shop>(s => s.AdminStatus == ShopAdminStatus.Pending)
                    .ToListAsync();

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
                var inactiveShops = await _unitOfWork.Repository
                    .GetQueryable<Shop>(s => s.AdminStatus == ShopAdminStatus.Inactive)
                    .ToListAsync();

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
                var allShops = await _unitOfWork.Repository
                    .GetQueryable<Shop>()
                    .ToListAsync();

                // Set successful response
                response.Success = true;
                response.Content = allShops;
                response.Title = "All Shops Retrieved Successfully";
                response.Description = "All shops have been retrieved from the database.";
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
                // Retrieve shops that match the given name
                var shops = await _unitOfWork.Repository
                    .GetQueryable<Shop>(s => s.ShopName.Contains(shopName))
                    .ToListAsync();

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
            // Count active shops
            var activeShopsCount = await _unitOfWork.Repository
                .GetQueryable<Shop>(s => s.AdminStatus == ShopAdminStatus.Active)
                .CountAsync();

            return activeShopsCount;
        }
        public async Task<int> GetPendingShopsCountAsync()
        {
            var pendingShopsCount = await _unitOfWork.Repository
                .GetQueryable<Shop>(s => s.AdminStatus == ShopAdminStatus.Pending)
                .CountAsync();

            return pendingShopsCount;
        }
        public async Task<int> GetInactiveShopsCountAsync()
        {
            var inactiveShopsCount = await _unitOfWork.Repository
                .GetQueryable<Shop>(s => s.AdminStatus == ShopAdminStatus.Inactive)
                .CountAsync();

            return inactiveShopsCount;
        }
        public async Task<ResponseModel<Shop>> UpdateShopStatusAsync(int shopId, ShopAdminStatus newStatus)
        {
            var response = new ResponseModel<Shop>();
            try
            {
                // Retrieve the existing shop from the repository
                var existingShop = await _unitOfWork.Repository
                    .GetQueryable<Shop>(s => s.ShopId == shopId)
                    .FirstOrDefaultAsync();

                // Check if the shop exists
                if (existingShop == null)
                {
                    response.Success = false;
                    response.Title = "Shop Not Found";
                    response.Description = $"Shop with ID '{shopId}' does not exist.";
                    return response;
                }

                // Update the shop's admin status
                existingShop.AdminStatus = newStatus;

                // Update shop in the repository
                _unitOfWork.Repository.Update(existingShop);
                await _unitOfWork.Repository.CompleteAsync();

                // Set successful response
                response.Success = true;
                response.Content = existingShop;
                response.Title = "Shop Status Updated Successfully";
                response.Description = $"Shop '{existingShop.ShopName}' status has been updated to '{newStatus}'.";
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
                // Retrieve the shop by owner ID from the repository
                var shop = await _unitOfWork.Repository
                    .GetQueryable<Shop>(s => s.OwnerId == ownerId)
                    .FirstOrDefaultAsync();

                // Check if the shop exists
                if (shop == null)
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

        public async Task<ResponseModel<string>> ChangeShopStatusByAdminAsync(int shopId, ShopAdminStatus newStatus)
        {
            var response = new ResponseModel<string>();
            var shop = await _unitOfWork.Repository.GetQueryable<Shop>(s => s.ShopId == shopId)
                .FirstOrDefaultAsync();

            if (shop == null)
            {
                response.Success = false;
                response.Title = "Shop Not Found";
                response.Description = $"No shop found with ID {shopId}.";
                return response;
            }

            // Update the shop status
            shop.AdminStatus = newStatus;

            try
            {
                // Save changes
                await _unitOfWork.Repository.UpdateAsync(shop);
                await _unitOfWork.Repository.CompleteAsync();

                response.Success = true;
                response.Title = "Shop Status Updated";
                response.Description = $"Shop status successfully updated to {newStatus}.";
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the update
                response.Success = false;
                response.Title = "Error Updating Shop Status";
                response.Description = "An error occurred while updating the shop status.";
                response.ExceptionMessage = ex.Message;
            }
            return response;
        }
    }
}