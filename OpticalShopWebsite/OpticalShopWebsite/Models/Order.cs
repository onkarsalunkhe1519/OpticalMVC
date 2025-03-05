using System;
using System.ComponentModel.DataAnnotations;

namespace OpticalShopWebsite.Models
{
    public class Order
    {
        public int Id { get; set; }

        // The purchased product's Id
        public int ProductId { get; set; }

        // User's email stored in session
        public string UserEmail { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        // e.g., "Success" or "Failed"
        public string PaymentStatus { get; set; }

        // Order amount (in INR)
        public decimal Amount { get; set; }
        public string OrderStatus { get; set; }
    }
}
