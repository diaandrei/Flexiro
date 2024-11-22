namespace Flexiro.Api.Auth
{
    public static class IdentityExtensions
    {
        public static bool CheckAdmin(this HttpContext context)
        {
            var isAdminClaim = context.User.Claims.SingleOrDefault(x => x.Type == "isAdmin");
            if (isAdminClaim != null && isAdminClaim.Value == "True")
            {
                return true;
            }
            return false;
        }
        public static bool CheckSeller(this HttpContext context)
        {
            var isSellerClaim = context.User.Claims.SingleOrDefault(x => x.Type == "isSeller");
            if (isSellerClaim != null && isSellerClaim.Value == "True")
            {
                return true;
            }
            return false;
        }

        // Retrieve the User ID from the claims
        public static Guid GetUserId(this HttpContext context)
        {
            var userIdClaim = context.User.Claims.SingleOrDefault(x => x.Type == "userId");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new InvalidOperationException("User ID is not valid or not found in the claims.");
            }

            return userId;
        }

        // Check if the user is authenticated and retrieve their User ID
        public static bool IsUserAuthenticated(this HttpContext context, out Guid? userId)
        {
            userId = null;

            if (context.User.Identity!.IsAuthenticated)
            {
                var userIdClaim = context.User.Claims.SingleOrDefault(x => x.Type == "userId");
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
                {
                    userId = parsedUserId;
                    return true;
                }
            }

            return false;
        }
    }
}