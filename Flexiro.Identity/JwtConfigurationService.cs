using Azure.Security.KeyVault.Secrets;

namespace Flexiro.Identity
{
    public class JwtConfigurationService
    {
        public string TokenSecret { get; }
        public string Issuer { get; }
        public string Audience { get; }

        public JwtConfigurationService(SecretClient secretClient)
        {
            TokenSecret = secretClient.GetSecret("JwtKey")?.Value?.Value!;
            Issuer = "https://id.flexiro.com";
            Audience = "https://flexiro.com";
        }
    }
}