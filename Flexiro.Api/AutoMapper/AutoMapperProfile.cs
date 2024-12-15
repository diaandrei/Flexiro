using AutoMapper;
using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;

namespace Flexiro.Api.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UpdateShopRequest, Shop>()
                 .ForMember(dest => dest.ShopId, opt => opt.Ignore())
                       .ForMember(dest => dest.ShopLogo, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore())
                .ForMember(dest => dest.Owner, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerName, opt => opt.Ignore())
                .ForMember(dest => dest.IsSeller, opt => opt.Ignore())
                .ForMember(dest => dest.AdminStatus, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            CreateMap<ProductCreateDto, Product>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
            .ForMember(dest => dest.MinimumPurchaseQuantity, opt => opt.MapFrom(src => src.MinimumPurchase))
            .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.Stock))
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.TotalSold, opt => opt.Ignore())
            .ForMember(dest => dest.QualityStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Shop, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Reviews, opt => opt.Ignore());

            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.MinimumPurchaseQuantity, opt => opt.MapFrom(src => src.MinimumPurchaseQuantity))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.StockQuantity))
                .ForMember(dest => dest.ProductImageUrls, opt => opt.MapFrom(src => src.ProductImages.Select(pi => pi.Path).ToList())); ;


            CreateMap<ProductUpdateDto, Product>()

            .ForMember(dest => dest.Tags, opt => opt.Condition(src => src.Tags != null))

            .ForMember(dest => dest.Tags, opt => opt.MapFrom((src, dest) =>
                src.Tags != null ? src.Tags : dest.Tags))
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.SKU, opt => opt.Ignore())
              .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
            .ForMember(dest => dest.QualityStatus, opt => opt.Ignore())
            .ForMember(dest => dest.ShopId, opt => opt.Ignore())
            .ForMember(dest => dest.Shop, opt => opt.Ignore())
            .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Reviews, opt => opt.Ignore())
            .ForMember(dest => dest.TotalSold, opt => opt.Ignore());

            CreateMap<UserWishlist, UserWishlistResponseDto>()
          .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
          .ForMember(dest => dest.ShopId, opt => opt.MapFrom(src => src.ShopId))
          .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
          .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
          .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForSourceMember(src => src.Product, opt => opt.DoNotValidate())
    .ForSourceMember(src => src.Shop, opt => opt.DoNotValidate())
    .ForSourceMember(src => src.User, opt => opt.DoNotValidate());


            CreateMap<Shop, ShopResponse>()
           .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products.ToList()))
           .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId))
           .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.OwnerName));

            CreateMap<AddUpdateShippingAddressRequest, ShippingAddress>()
                      .ForMember(dest => dest.ShippingAddressId, opt => opt.Ignore())
                      .ForMember(dest => dest.User, opt => opt.Ignore())
                      .ForMember(dest => dest.UserId, opt => opt.Ignore());

            CreateMap<ShippingAddress, ShippingAddressResponseDto>()
                    .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                    .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                    .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                    .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                    .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
                    .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.ZipCode))
                    .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                    .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                    .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.Note));

            CreateMap<ShippingAddressResponseDto, ShippingAddress>()
                .ForMember(dest => dest.ShippingAddressId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.AddToAddressBook, opt => opt.Ignore());
        }
    }
}