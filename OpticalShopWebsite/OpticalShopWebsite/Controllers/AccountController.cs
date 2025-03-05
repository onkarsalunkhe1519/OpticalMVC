using Microsoft.AspNetCore.Mvc;
using OpticalShopWebsite.Data;
using OpticalShopWebsite.Models;

namespace OpticalShopWebsite.Controllers
{
    public class AccountController : Controller
    {

        private readonly ApplicationDbContext db;
        public AccountController(ApplicationDbContext db)
        {
            this.db = db;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Account()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Shop()
        {
            return View();
        }

        public IActionResult Glasses()
        {
            return View();
        }



        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User u)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(u);
                db.SaveChanges();
                return RedirectToAction("Account","Account");
            }
            return View(u);
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(User u)
        {
            // Remove validation errors for properties not used in login
            ModelState.Remove("Username");
            ModelState.Remove("Role");

            
                var user = db.Users.FirstOrDefault(x => x.Email == u.Email && x.Password == u.Password);
                if (user != null)
                {
                    // Set session variables
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserRole", user.Role);

                    // Redirect based on role
                    if (user.Role == "Employee")
                    {
                        return RedirectToAction("Index", "EyeReport");
                    }
                    else if (user.Role == "User")
                    {
                        return RedirectToAction("Index", "Product");
                    }
                    else if (user.Role == "Admin")
                    {
                        return RedirectToAction("EmployeePayslips", "Admin");
                    }

                    return RedirectToAction("Account", "Account");  // Default fallback
                }

                ModelState.AddModelError("", "Invalid email or password.");
            
            return View(u);
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();  // Clear all session data
            return RedirectToAction("Account", "Account");  // Redirect to login page
        }

    }
    }

