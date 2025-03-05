using System;
using System.ComponentModel.DataAnnotations;

namespace OpticalShopWebsite.Models
{
    public class EyeReport
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string CustomerEmail { get; set; }

        public DateTime ReportDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Left Cylinder value is required.")]
        public decimal LeftCylinder { get; set; }

        [Required(ErrorMessage = "Right Cylinder value is required.")]
        public decimal RightCylinder { get; set; }
    }
}
