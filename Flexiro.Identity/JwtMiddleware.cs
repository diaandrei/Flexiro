using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Flexiro.Identity
{
    public class JwtMiddleware
    {
        private readonly IConfiguration _configuration;
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next, IConfiguration config)
        {
            _configuration = config;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                AttachAccountToContext(context, token);
            }

            await _next(context);
        }

        private void AttachAccountToContext(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var claims = jwtToken.Claims.ToList();

                // Attach claims to the context
                var identity = new ClaimsIdentity(claims, "jwt");
                context.User = new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
