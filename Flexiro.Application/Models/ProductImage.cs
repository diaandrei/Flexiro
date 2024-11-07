using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flexiro.Application.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Path { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Product Product { get; set; }
    }
}