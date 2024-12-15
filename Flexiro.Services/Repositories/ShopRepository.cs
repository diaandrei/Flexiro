using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Microsoft.EntityFrameworkCore;
using Flexiro.Application.DTOs;
using Flexiro.Services.Services.Interfaces;

namespace Flexiro.Services.Repositories
{
    public class ShopRepository : IShopRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBlobStorageService _blobStorageService;
        public ShopRepository(IUnitOfWork unitOfWork, IMapper mapper, IBlobStorageService blobStorageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _blobStorageService = blobStorageService;
        }

        public async Task<Shop> CreateShopAsync(Shop createShopRequest)
        {
            try
            {
                // Map the create request to a new Shop entity
                var newShop = _mapper.Map<Shop>(createShopRequest);

                // Add the new shop to the repository
                await _unitOfWork.Repository.AddAsync(newShop);
                await _unitOfWork.Repository.CompleteAsync();

                // Return the created shop entity
                return newShop;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating the shop.", ex);
            }
        }

        public async Task<Shop> UpdateShopAsync(UpdateShopRequest updateShopRequest)
        {
            // Retrieve the existing shop 
            var existingShop = await _unitOfWork.Repository
                .GetQueryable<Shop>(s => s.ShopId == updateShopRequest.ShopId)
                .FirstOrDefaultAsync();

            // Check if the shop exists
            if (existingShop == null)
            {
                throw new KeyNotFoundException($"Shop with ID '{updateShopRequest.ShopId}' does not exist.");
            }

            // Handle logo update if a new logo is provided
            if (!string.IsNullOrEmpty(updateShopRequest.NewLogoPath))
            {
                // Delete the old logo from Blob Storage
                if (!string.IsNullOrEmpty(existingShop.ShopLogo))
                {
                    await _blobStorageService.DeleteImageAsync(existingShop.ShopLogo);
                }

                // Update the shop logo path with the new file name
                existingShop.ShopLogo = updateShopRequest.NewLogoPath;
            }

            _mapper.Map(updateShopRequest, existingShop);

            // Update the shop in the repository
            _unitOfWork.Repository.Update(existingShop);
            await _unitOfWork.Repository.CompleteAsync();

            return existingShop;
        }

        public async Task<Shop> GetShopByIdAsync(int shopId)
        {
            return (await _unitOfWork.Repository
                .GetQueryable<Shop>(s => s.ShopId == shopId)
                .FirstOrDefaultAsync())!;
        }

        public async Task<IList<Shop>> GetActiveShopsAsync()
        {
            return await _unitOfWork.Repository
                .GetQueryable<Shop>(s => s.AdminStatus == ShopAdminStatus.Active)
                .ToListAsync();
        }

        public async Task<IList<Shop>> GetPendingShopsAsync()
        {
            return await _unitOfWork.Repository
                .GetQueryable<Shop>(s => s.AdminStatus == ShopAdminStatus.Pending)
                .ToListAsync();
        }

        public async Task<IList<Shop>> GetInactiveShopsAsync()
        {
            return await _unitOfWork.Repository
                .GetQueryable<Shop>(s => s.AdminStatus == ShopAdminStatus.Inactive)
                .ToListAsync();
        }

        public async Task<IList<Shop>> GetAllShopsAsync()
        {
            return await _unitOfWork.Repository
                .GetQueryable<Shop>()
                .ToListAsync();
        }

        public async Task<IList<Shop>> SearchShopsByNameAsync(string shopName)
        {
            return await _unitOfWork.Repository
                .GetQueryable<Shop>(s => s.ShopName.Contains(shopName))
                .ToListAsync();
        }

        public async Task<int> GetActiveShopsCountAsync()
        {
            return await _unitOfWork.Repository
                .GetQueryable<Shop>(s => s.AdminStatus == ShopAdminStatus.Active)
                .CountAsync();
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

        public async Task<Shop> UpdateShopStatusAsync(int shopId, ShopAdminStatus newStatus)
        {
            // Retrieve the shop by the Id
            var shop = await _unitOfWork.Repository
                .GetQueryable<Shop>(s => s.ShopId == shopId)
                .FirstOrDefaultAsync();

            if (shop == null)
            {
                return null!; // Return null if the shop does not exist
            }

            // Update the shop's admin status
            shop.AdminStatus = newStatus;

            // Save the updated shop in the repository
            _unitOfWork.Repository.Update(shop);
            await _unitOfWork.Repository.CompleteAsync();

            return shop;
        }

        public async Task<ShopResponse> GetShopByOwnerIdAsync(string ownerId)
        {
            // Use a join to retrieve data from both Shop and ApplicationUser
            var result = await _unitOfWork.Repository
                .GetQueryable<Shop>(s => s.OwnerId == ownerId)
                .Select(shop => new ShopResponse
                {
                    ShopId = shop.ShopId,
                    ShopName = shop.ShopName,
                    ShopDescription = shop.ShopDescription,
                    ShopLogo = shop.ShopLogo,
                    AdminStatus = shop.AdminStatus,
                    SellerStatus = shop.SellerStatus,
                    Slogan = shop.Slogan,
                    OpeningDate = shop.OpeningDate,
                    OpeningTime = shop.OpeningTime,
                    ClosingTime = shop.ClosingTime,
                    CreatedAt = shop.CreatedAt,
                    Email = shop.Owner.Email!,
                    PhoneNumber = shop.Owner.PhoneNumber!,
                    OpeningDay = shop.OpeningDay,
                    ClosingDay = shop.ClosingDay
                })
                .FirstOrDefaultAsync();

            return result!;
        }


        public async Task<bool> ChangeShopStatusAsync(int shopId, int newStatus)
        {
            // Retrieve the shop by ID
            var shop = await _unitOfWork.Repository.GetQueryable<Shop>(s => s.ShopId == shopId).FirstOrDefaultAsync();

            if (shop == null) return false;

            // Update the shop status
            shop.AdminStatus = (ShopAdminStatus)newStatus;

            // Save changes
            await _unitOfWork.Repository.UpdateAsync(shop);
            await _unitOfWork.Repository.CompleteAsync();

            return true;
        }

        public async Task<bool> UpdateShopSellerStatusAsync(ShopStatus newStatus)
        {
            var shop = await _unitOfWork.Repository
                .GetQueryable<Shop>(s => s.ShopId == newStatus.ShopId)
                .FirstOrDefaultAsync();

            if (shop == null)
                return false;

            if (newStatus.NewStatus.HasValue)
                shop.SellerStatus = (ShopSellerStatus)newStatus.NewStatus.Value;

            if (!string.IsNullOrWhiteSpace(newStatus.ClosingDay))
                shop.ClosingDay = newStatus.ClosingDay;

            if (!string.IsNullOrWhiteSpace(newStatus.OpeningDay))
                shop.OpeningDay = newStatus.OpeningDay;

            if (!string.IsNullOrWhiteSpace(newStatus.ClosingTime))
                shop.ClosingTime = newStatus.ClosingTime;

            if (!string.IsNullOrWhiteSpace(newStatus.OpeningTime))
                shop.OpeningTime = newStatus.OpeningTime;
            await _unitOfWork.Repository.UpdateAsync(shop);
            await _unitOfWork.Repository.CompleteAsync();

            return true;
        }
    }
}
