using System.ComponentModel.DataAnnotations;

namespace Flexiro.Contracts.Requests
{
    public class UpdateDiscountDto
    {
        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100.")]
        public decimal DiscountPercentage { get; set; }
    }
}
