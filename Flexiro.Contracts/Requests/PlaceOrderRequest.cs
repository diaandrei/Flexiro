using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flexiro.Contracts.Requests
{
    public class PlaceOrderRequest
    {
        public required string UserId { get; set; }
        public required AddUpdateShippingAddressRequest ShippingAddress { get; set; }
    }

}