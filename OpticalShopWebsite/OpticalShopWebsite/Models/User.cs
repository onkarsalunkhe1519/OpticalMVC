using System.ComponentModel.DataAnnotations;

namespace OpticalShopWebsite.Models
{
    public class User
    {
        public int Id { get; set; } // For demonstration purposes

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
    }
}
