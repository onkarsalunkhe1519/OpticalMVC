using System;
using System.ComponentModel.DataAnnotations;

namespace OpticalShopWebsite.Models
{
    public class Salary
    {
        public int Id { get; set; } // Primary Key

        [Required(ErrorMessage = "Employee email is required.")]
        public string EmployeeEmail { get; set; } // No Foreign Key

        [Required(ErrorMessage = "Month is required.")]
        public string Month { get; set; }

        [Required(ErrorMessage = "Year is required.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Salary amount is required.")]
        public decimal Amount { get; set; }

        public DateTime DateGenerated { get; set; } = DateTime.Now;

        public string PayslipPath { get; set; } // Path to the PDF
    }
}
