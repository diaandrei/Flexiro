using AutoMapper;
using EasyRepository.EFCore.Generic;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Microsoft.EntityFrameworkCore;

namespace Flexiro.Services.Repositories
{
    public class ShippingRepository : IShippingRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShippingRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ShippingAddress> GetShippingAddressByUserIdAsync(string userId)
        {
            return (await _unitOfWork.Repository
                .GetQueryable<ShippingAddress>(a => a.UserId == userId)
                .FirstOrDefaultAsync())!;
        }

        public async Task<ShippingAddress> AddShippingAddressAsync(AddUpdateShippingAddressRequest request)
        {
            var newAddress = _mapper.Map<ShippingAddress>(request);

            await _unitOfWork.Repository.AddAsync(newAddress);
            await _unitOfWork.Repository.CompleteAsync();

            return newAddress;
        }

        public async Task<ShippingAddress> UpdateShippingAddressAsync(int addressId, AddUpdateShippingAddressRequest request)
        {
            var existingAddress = await _unitOfWork.Repository
                .GetQueryable<ShippingAddress>(a => a.ShippingAddressId == addressId)
                .FirstOrDefaultAsync();

            if (existingAddress == null)
            {
                return null!; // Returning null if the address does not exist
            }

            // Map the request data into the existing address
            _mapper.Map(request, existingAddress);

            // Update the address in the database
            _unitOfWork.Repository.Update(existingAddress);
            await _unitOfWork.Repository.CompleteAsync();

            return existingAddress;
        }

        public async Task<IList<ShippingAddress>> GetAddressBookByUserIdAsync(string userId)
        {
            return await _unitOfWork.Repository
                .GetQueryable<ShippingAddress>(a => a.UserId == userId)
                .ToListAsync();
        }
    }
}