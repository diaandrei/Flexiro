using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Flexiro.Application.Models;
using Flexiro.Contracts.Responses;
using Flexiro.Application.DTOs;
using LoginRequest = Flexiro.Contracts.Requests.LoginRequest;
using RegisterRequest = Flexiro.Contracts.Requests.RegisterRequest;
using Flexiro.Identity;
using Flexiro.Contracts.Requests;
using Flexiro.Services.Services.Interfaces;

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

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, IConfiguration config, IShopService shopService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _config = config;
            _shopService = shopService;
        }

        [HttpPost("Register")]
        public async Task<ResponseModel<string>> Register([FromBody] RegisterRequest model)
        {
            ResponseModel<string> response = new ResponseModel<string>();

            try
            {
                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.Username,
                        Email = model.Email,
                        IsAdmin = false,
                        IsSeller = false,
                        CreatedAt = DateTime.Now
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        var roleExists = await _roleManager.RoleExistsAsync("Customer");

                        if (!roleExists)
                        {
                            var roleResult = await _roleManager.CreateAsync(new IdentityRole("Customer"));

                            if (!roleResult.Succeeded)
                            {
                                response.Title = "Error creating user role";
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
                            response.Title = "User role assignment failed.";
                        }
                    }
                    else
                    {
                        response.Title = string.Join(", ", result.Errors.Select(e => e.Description));
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

        [HttpPost("Login")]
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
                            var role = roles.FirstOrDefault(); // Assuming one role per user, adjust as needed
                            var roleId = string.Empty;

                            if (!string.IsNullOrEmpty(role))
                            {
                                var roleObj = await _roleManager.FindByNameAsync(role);
                                roleId = roleObj?.Id;
                            }

                            TokenGenerationRequest jwtReq = new TokenGenerationRequest
                            {
                                UserId = user.Id,
                                Email = model.Email,
                                RoleId = roleId!,
                                IsAdmin = user.IsAdmin,
                                IsSeller = user.IsSeller,
                            };

                            var token = JwtTokenGenerator.GenerateToken(jwtReq);

                            LoginDto login = new LoginDto
                            {
                                Token = token,
                                IsAdmin = user.IsAdmin,
                                IsSeller = user.IsSeller,
                                Name = user.FirstName + " " + user.LastName,
                                Email = user.Email!
                            };

                            response.Content = login;
                            response.Success = true;
                            response.Title = "Success! You’re now logged in.";
                        }
                        else
                        {
                            response.Title = "Incorrect email or password.";
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

                if (user == null)
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
                        response.Title = "Failed to create user account";
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

                    string logoPath = null!;

                    if (model.ShopLogo != null!)
                    {
                        // Generate a unique file name and path
                        var fileName = $"{Guid.NewGuid()}_{model.ShopLogo.FileName}";
                        var filePath = Path.Combine("wwwroot/uploads/logos", fileName);
                        if (!Directory.Exists(Path.Combine("wwwroot", "uploads", "logos")))
                        {
                            Directory.CreateDirectory(Path.Combine("wwwroot", "uploads", "logos"));
                        }
                        // Save the file to the server
                        await using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ShopLogo.CopyToAsync(stream);
                        }

                        logoPath = $"/uploads/logos/{fileName}";
                    }

                    // 3. Create the shop linked to the user
                    var shop = new Shop
                    {
                        OwnerId = user.Id,
                        OwnerName = model.OwnerName,
                        ShopName = model.StoreName,
                        ShopDescription = model.StoreDescription,
                        ShopLogo = logoPath,
                        AdminStatus = ShopAdminStatus.Pending,
                        SellerStatus = ShopSellerStatus.Open,
                        Slogan = model.Slogan,
                        OpeningDate = model.OpeningDate,
                        OpeningTime = model.OpeningTime,
                        ClosingTime = model.ClosingTime,
                        CreatedAt = DateTime.Now,
                        IsSeller = true
                    };

                    // Save the shop to the database
                    var resultShop = await _shopService.CreateShopAsync(shop);

                    response.Success = true;
                    response.Title = "Seller registered successfully";
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