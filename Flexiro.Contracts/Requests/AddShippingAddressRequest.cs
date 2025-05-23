﻿namespace Flexiro.Contracts.Requests
{
    public class AddUpdateShippingAddressRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Address { get; set; }
        public required string City { get; set; }
        public required string Postcode { get; set; }
        public required string Country { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Note { get; set; }
        public bool AddToAddressBook { get; set; }
    }
}