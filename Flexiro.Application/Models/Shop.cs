using System.ComponentModel.DataAnnotations;

namespace Flexiro.Application.Models
{
    public class Shop
    {
        [Key]
        public int ShopId { get; set; }
        public string OwnerId { get; set; }
        public virtual ApplicationUser Owner { get; set; }
        public string OwnerName { get; set; }
        public string ShopName { get; set; }
        public string ShopDescription { get; set; }
        public string ShopLogo { get; set; }
        public bool IsSeller { get; set; }
        public ShopAdminStatus AdminStatus { get; set; }
        public ShopSellerStatus SellerStatus { get; set; }
        public string Slogan { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public DateTime OpeningDate { get; set; }
        public string OpeningTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClosingTime { get; set; }

    }
    public enum ShopAdminStatus
    {
        Pending,
        Active,
        Inactive
    }
    public enum ShopSellerStatus
    {
        Open,
        Closed
    }

}