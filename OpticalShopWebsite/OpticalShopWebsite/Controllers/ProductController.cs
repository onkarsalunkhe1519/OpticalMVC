using Microsoft.AspNetCore.Mvc;
using OpticalShopWebsite.Data;
using OpticalShopWebsite.Models;
using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace OpticalShopWebsite.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            this.db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product, IFormFile productImageFile)
        {
            
                // Handle file upload if provided.
                if (productImageFile != null && productImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "ProductImages");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + productImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        productImageFile.CopyTo(fileStream);
                    }
                    product.ProductImage = "ProductImages/" + uniqueFileName;
                }
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("EmployeeProducts","Orders");
           
        }

        // GET: /Product/Index
        // Display products as cards.
        public IActionResult Index()
        {
            var products = db.Products.ToList();
            return View(products);
        }
    }
}
