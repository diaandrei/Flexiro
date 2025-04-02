using Flexiro.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Azure.Security.KeyVault.Secrets;

namespace Flexiro.Identity
{
    public static class JwtTokenGenerator
    {
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromDays(365);

        private static SecretClient? _secretClient;

        public static void Initialize(SecretClient secretClient)
        {
            _secretClient = secretClient;
        }

        public static string GenerateToken(TokenGenerationRequest request)
        {
            if (_secretClient == null)
            {
                throw new InvalidOperationException("JwtTokenGenerator has not been initialized with SecretClient. Call Initialize() first.");
            }

            string tokenSecret = _secretClient.GetSecret("JwtKey").Value.Value;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(tokenSecret);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, request.Email),
                new(JwtRegisteredClaimNames.Email, request.Email),
                new("userId", request.UserId.ToString()),
                new("roleId", request.RoleId),
                new("isTrustedMember", request.IsTrustedMember.ToString()),
                new("isAdmin", request.IsAdmin.ToString()),
                new("IsSeller", request.IsSeller.ToString()),
            };

            foreach (var claimPair in request.CustomClaims)
            {
                var jsonElement = (JsonElement)claimPair.Value;
                var valueType = jsonElement.ValueKind switch
                {
                    JsonValueKind.True => ClaimValueTypes.Boolean,
                    JsonValueKind.False => ClaimValueTypes.Boolean,
                    JsonValueKind.Number => ClaimValueTypes.Double,
                    _ => ClaimValueTypes.String
                };
                var claim = new Claim(claimPair.Key, claimPair.Value.ToString()!, valueType);
                claims.Add(claim);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TokenLifetime),
                Issuer = "https://id.flexiro.com",
                Audience = "https://flexiro.com",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}