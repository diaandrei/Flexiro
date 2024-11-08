using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Flexiro.Application.Models;
using Flexiro.Contracts.Responses;
using Flexiro.Application.DTOs;
using LoginRequest = Flexiro.Contracts.Requests.LoginRequest;
using RegisterRequest = Flexiro.Contracts.Requests.RegisterRequest;
using Flexiro.Identity;

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

        public AccountController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _config = config;
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
        public async Task<ResponseModel<LoginDTO>> Login([FromBody] LoginRequest model)
        {
            ResponseModel<LoginDTO> response = new ResponseModel<LoginDTO>
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

                            LoginDTO login = new LoginDTO
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
    }
}