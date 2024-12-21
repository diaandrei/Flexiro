using Flexiro.Application.DTOs;
using Flexiro.Application.Models;
using Flexiro.Contracts.Requests;
using Flexiro.Contracts.Responses;
using Flexiro.Identity;
using Flexiro.Services.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = Flexiro.Contracts.Requests.LoginRequest;
using RegisterRequest = Flexiro.Contracts.Requests.RegisterRequest;

namespace Flexiro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        public readonly IShopService _shopService;
        public readonly IBlobStorageService _blobStorageService;

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, IConfiguration config, IShopService shopService, IBlobStorageService blobStorageService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _config = config;
            _shopService = shopService;
            _blobStorageService = blobStorageService;
        }

        [HttpPost("register")]
        public async Task<ResponseModel<string>> Register([FromBody] RegisterRequest model)
        {
            ResponseModel<string> response = new ResponseModel<string>();

            try
            {
                if (ModelState.IsValid)
                {
                    var newUser = await _userManager.FindByEmailAsync(model.Email);

                    if (newUser == null)
                    {
                        var user = new ApplicationUser
                        {

                            UserName = model.Username,
                            Email = model.Email,
                            IsAdmin = false,
                            IsSeller = false,
                        };

                        var result = await _userManager.CreateAsync(user, model.Password);

                        if (!result.Succeeded)
                        {
                            response.Success = false;
                            response.Title = string.Join(", ", result.Errors.Select(e => e.Description));
                            response.Description = string.Join(", ", result.Errors.Select(e => e.Description));
                            return response;
                        }

                        if (result.Succeeded)
                        {
                            var roleExists = await _roleManager.RoleExistsAsync("Customer");

                            if (!roleExists)
                            {
                                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Customer"));

                                if (!roleResult.Succeeded)
                                {
                                    response.Title = "Error creating user role";
                                    response.Description = string.Join(", ", result.Errors.Select(e => e.Description));
                                    return response;
                                }
                            }

                            // Assign the role to the user
                            var addToRoleResult = await _userManager.AddToRoleAsync(user, "Customer");

                            if (addToRoleResult.Succeeded)
                            {
                                response.Title = "Signed up successfully";
                                response.Success = true;
                            }
                            else
                            {
                                response.Title = "Error assigning user role";
                            }
                        }
                        else
                        {
                            response.Title = string.Join(", ", result.Errors.Select(e => e.Description));
                        }
                    }

                    else
                    {
                        response.Title = "An account with this email already exists.";
                    }
                }
                else
                {
                    response.Title = "Invalid model state";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }

            return response;
        }

        [HttpPost("login")]
        public async Task<ResponseModel<LoginDto>> Login([FromBody] LoginRequest model)
        {
            ResponseModel<LoginDto> response = new ResponseModel<LoginDto>
            {
                Title = "Something went wrong.",
                Success = false
            };

            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    if (user != null)
                    {
                        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, false, false);

                        if (result.Succeeded)
                        {
                            // Get the roles of the user
                            var roles = await _userManager.GetRolesAsync(user);
                            var role = roles.FirstOrDefault();

                            string userRole = role?.ToLower() switch
                            {
                                "admin" => "Admin",
                                "seller" => "Seller",
                                "customer" => "Customer",
                                _ => "Customer"
                            };

                            // Generate JWT token
                            var jwtReq = new TokenGenerationRequest
                            {
                                UserId = user.Id,
                                Email = model.Email,
                                RoleId = (!string.IsNullOrEmpty(role) ? (await _roleManager.FindByNameAsync(role))?.Id : string.Empty)!,
                                IsAdmin = user.IsAdmin,
                                IsSeller = user.IsSeller,
                            };

                            var token = JwtTokenGenerator.GenerateToken(jwtReq);

                            // Build the login DTO based on role
                            LoginDto login;
                            if (userRole == "Seller")
                            {
                                // Fetch shop details for seller
                                var shop = await _shopService.GetShopByOwnerIdAsync(user.Id);
                                if (shop?.Content == null)
                                {
                                    response.Title = "Shop not found for the seller.";
                                    return response;
                                }

                                login = new LoginDto
                                {
                                    Token = token,
                                    Id = user.Id,
                                    IsAdmin = user.IsAdmin,
                                    Role = userRole,
                                    IsSeller = true,
                                    Name = $"{user.FirstName} {user.LastName}",
                                    Email = user.Email!,
                                    AdditionalInfo = new SellerLoginDto
                                    {
                                        ShopId = shop.Content.ShopId,
                                        OwnerName = shop.Content.OwnerName,
                                        ShopName = shop.Content.ShopName
                                    }
                                };
                            }
                            else
                            {
                                login = new LoginDto
                                {
                                    Token = token,
                                    Id = user.Id,
                                    IsAdmin = user.IsAdmin,
                                    Role = userRole,
                                    IsSeller = user.IsSeller,
                                    Name = $"{user.FirstName} {user.LastName}",
                                    Email = user.Email!
                                };
                            }

                            response.Content = login;
                            response.Success = true;
                            response.Title = "Login successful";
                        }
                        else
                        {
                            response.Title = "Incorrect email or password. Please try again.";
                        }
                    }
                    else
                    {
                        response.Title = "User not found.";
                    }
                }
                else
                {
                    response.Title = "Invalid model state.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }

            return response;
        }

        [HttpPost("RegisterSeller")]
        public async Task<ResponseModel<string>> RegisterSeller([FromForm] RegisterSellerRequest model)
        {
            ResponseModel<string> response = new ResponseModel<string>();

            try
            {
                // 1. Check if a user with this email already exist
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user is null)
                {
                    // Create new ApplicationUser for the seller
                    user = new ApplicationUser
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        FirstName = model.OwnerName,
                        PhoneNumber = model.ContactNo,
                        Country = model.Country,
                        City = model.City,
                        ZipCode = model.ZipCode,
                        IsSeller = true,
                        CreatedAt = DateTime.Now
                    };

                    // Create the user account
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (!result.Succeeded)
                    {
                        response.Success = false;
                        response.Title = "Failed to create your account. Please try again.";
                        response.Description = string.Join(", ", result.Errors.Select(e => e.Description));
                        return response;
                    }

                    // 2. Assign the "Seller" role if not already assigned
                    if (!await _userManager.IsInRoleAsync(user, "Seller"))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, "Seller");
                        if (!roleResult.Succeeded)
                        {
                            response.Success = false;
                            response.Title = "Failed to assign seller role";
                            response.Description = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                            return response;
                        }
                    }

                    string logoUrl = null!;

                    if (model.ShopLogo != null!)
                    {
                        await using var stream = model.ShopLogo.OpenReadStream();

                        var imageUrl = await _blobStorageService.UploadImageAsync(stream, model.ShopLogo.FileName);
                        logoUrl = imageUrl;
                    }

                    // 3. Create the shop linked to the user
                    var shop = new Shop
                    {
                        OwnerId = user.Id,
                        OwnerName = model.OwnerName,
                        ShopName = model.StoreName,
                        ShopDescription = model.StoreDescription,
                        ShopLogo = logoUrl,
                        AdminStatus = ShopAdminStatus.Pending,
                        SellerStatus = ShopSellerStatus.Open,
                        Slogan = model.Slogan,
                        OpeningDate = model.OpeningDate,
                        OpeningTime = model.OpeningTime,
                        ClosingTime = model.ClosingTime,
                        CreatedAt = DateTime.UtcNow,
                        IsSeller = true,
                        OpeningDay = model.OpeningDay,
                        ClosingDay = model.ClosingDay,
                    };

                    // Save the shop to the database
                    var resultShop = await _shopService.CreateShopAsync(shop);

                    response.Success = true;
                    response.Title = "Seller registration was successful.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Registration failed";
                response.Description = ex.Message;
            }

            return response;
        }

        [HttpPut("UpdateSellerContactInfo")]
        public async Task<ResponseModel<ApplicationUser>> UpdateSellerContactInfo([FromBody] UpdateContactInfoRequest model)
        {
            var response = new ResponseModel<ApplicationUser>();

            try
            {
                // Find the seller by ID
                var user = await _userManager.FindByIdAsync(model.SellerId);

                if (user == null)
                {
                    response.Success = false;
                    response.Title = "Seller Not Found";
                    response.Description = $"Seller with ID '{model.SellerId}' does not exist.";
                    return response;
                }

                // Check if the provided email is different from the seller's current email
                if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
                {
                    // Check if the email already exists for another user
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null && existingUser.Id != model.SellerId)
                    {
                        response.Success = false;
                        response.Title = "Email Already Exists";
                        response.Description = $"The email '{model.Email}' is already associated with another account.";
                        return response;
                    }
                }

                // Update only if there are changes
                bool isUpdated = false;

                if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = model.Email;
                    isUpdated = true;
                }

                if (!string.Equals(user.PhoneNumber, model.PhoneNumber, StringComparison.OrdinalIgnoreCase))
                {
                    user.PhoneNumber = model.PhoneNumber;
                    isUpdated = true;
                }

                if (!isUpdated)
                {
                    response.Success = false;
                    response.Title = "No Changes Detected";
                    response.Description = "No changes were detected in the provided information.";
                    return response;
                }

                // Update the user
                var updateResult = await _userManager.UpdateAsync(user);

                // Handle potential errors during update
                if (!updateResult.Succeeded)
                {
                    response.Success = false;
                    response.Title = "Update Failed";
                    response.Description = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    return response;
                }

                // Set successful response
                response.Success = true;
                response.Content = user;
                response.Title = "Contact Info Updated";
                response.Description = "Seller contact information has been updated successfully.";
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                response.Success = false;
                response.Title = "Error Updating Contact Info";
                response.Description = "An unexpected error occurred while updating contact information.";
                response.ExceptionMessage = ex.Message;
            }

            return response;
        }

        [HttpGet("UserDetails")]
        [Authorize]
        public async Task<ResponseModel<UserDetailResponse>> GetUserDetails(string userId)
        {
            var response = new ResponseModel<UserDetailResponse>();

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    response.Success = false;
                    response.Title = "User not found";
                    return response;
                }

                // Prepare the response model with limited user details
                response.Content = new UserDetailResponse
                {
                    Email = user.Email!,
                    FirstName = user.FirstName!,
                    LastName = user.LastName!,
                    Address = user.Address,
                    Country = user.Country,

                };
                response.Success = true;
                response.Title = "User details retrieved successfully";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = "Error fetching user details";
                response.Description = ex.Message;
            }
            return response;
        }
    }
}