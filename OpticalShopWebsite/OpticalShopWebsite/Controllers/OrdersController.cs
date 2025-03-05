using Microsoft.AspNetCore.Mvc;
using OpticalShopWebsite.Data;
using OpticalShopWebsite.Models;
using Razorpay.Api; // Install Razorpay .NET SDK via NuGet
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Net;
using System.Net.Mail;
using Product = OpticalShopWebsite.Models.Product;

namespace OpticalShopWebsite.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext db;

        // SMTP credentials for sending invoice email
        private readonly string smtpEmail = "onkyasalunkhe@gmail.com";
        private readonly string smtpPassword = "bddhfhgjscxjlxpk";

        // Razorpay credentials (Test credentials used here)
        private readonly string razorpayKey = "rzp_test_Kl7588Yie2yJTV";
        private readonly string razorpaySecret = "6dN9Nqs7M6HPFMlL45AhaTgp";

        public OrdersController(ApplicationDbContext db)
        {
            this.db = db;
        }

        // GET: /Orders/Checkout/{productId}
        public IActionResult Checkout(int productId)
        {
            var product = db.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return NotFound();

            // Ensure user is logged in (UserEmail is stored in session)
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Account", "Account");

            // Create a Razorpay order on the server
            RazorpayClient client = new RazorpayClient(razorpayKey, razorpaySecret);
            Dictionary<string, object> options = new Dictionary<string, object>();
            int amountInPaise = (int)(product.Price * 100); // Amount in paise
            options.Add("amount", amountInPaise);
            options.Add("currency", "INR");
            options.Add("receipt", $"order_rcptid_{productId}_{DateTime.Now.Ticks}");
            options.Add("payment_capture", 1);

            Razorpay.Api.Order orderObj = client.Order.Create(options);

            // Pass order details to the view via ViewBag
            ViewBag.RazorpayOrderId = orderObj["id"];
            ViewBag.Amount = amountInPaise;
            ViewBag.RazorpayKey = razorpayKey;
            ViewBag.Product = product;
            ViewBag.UserEmail = userEmail;

            return View();
        }
        public IActionResult UserOrders()
        {
            // Ensure user is logged in
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Account", "Account"); // Redirect to login if not logged in
            }

            // Fetch only orders belonging to the logged-in user
            var orders = db.Orders
                           .Where(o => o.UserEmail == userEmail)
                           .OrderByDescending(o => o.OrderDate) // Order by latest orders first
                           .ToList();

            // Attach product details to orders
            foreach (var order in orders)
            {
                var product = db.Products.FirstOrDefault(p => p.Id == order.ProductId);
                if (product != null)
                {
                    ViewData[$"Product_{order.Id}"] = product; // Store product in ViewData
                }
            }

            return View(orders); // Return filtered orders to the view
        }

        // POST: /Orders/PaymentSuccess
        // This action is called via AJAX after a successful payment.
        [HttpPost]
        public IActionResult PaymentSuccess(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature, int productId)
        {
            // Verify the signature to ensure payment data integrity
            string payload = razorpay_order_id + "|" + razorpay_payment_id;
            string generatedSignature = GenerateSignature(payload, razorpaySecret);

            // Log values for debugging (remove these logs in production)
            System.Diagnostics.Debug.WriteLine("Payload: " + payload);
            System.Diagnostics.Debug.WriteLine("Generated Signature: " + generatedSignature);
            System.Diagnostics.Debug.WriteLine("Razorpay Signature: " + razorpay_signature);

            


            var product = db.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return Json(new { status = "failed", message = "Invalid product" });

            // Create an order record
            Models.Order order = new Models.Order
            {
                ProductId = productId,
                UserEmail = HttpContext.Session.GetString("UserEmail"),
                PaymentStatus = "Success",
                Amount = product.Price,
                OrderDate = DateTime.Now,
                OrderStatus="Pending"
            };
            db.Orders.Add(order);
            db.SaveChanges();

            // Update product stock
            product.CurrentStock -= 1;
            db.SaveChanges();

            // Generate invoice PDF and send it via email
            string invoicePath = GenerateInvoicePdf(order, product);
            SendInvoiceEmail(order.UserEmail, invoicePath);

            return Json(new { status = "success", orderId = order.Id });
        }

        // Helper function to generate HMAC SHA256 signature
        private string GenerateSignature(string data, string secret)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] hash = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        // Generates a PDF invoice using iTextSharp and returns the file path.
        private string GenerateInvoicePdf(Models.Order order, Product product)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Invoices");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"Invoice_{order.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);

            Document document = new Document(PageSize.A4);
            PdfWriter.GetInstance(document, new FileStream(fullPath, FileMode.Create));
            document.Open();

            Font titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
            Paragraph title = new Paragraph("Invoice", titleFont) { Alignment = Element.ALIGN_CENTER };
            document.Add(title);
            document.Add(new Paragraph("\n"));

            Font bodyFont = FontFactory.GetFont("Arial", 12, Font.NORMAL);
            document.Add(new Paragraph($"Invoice for Order ID: {order.Id}", bodyFont));
            document.Add(new Paragraph($"Order Date: {order.OrderDate:dd MMM yyyy}", bodyFont));
            document.Add(new Paragraph($"Product: {product.Name}", bodyFont));
            document.Add(new Paragraph($"Amount: ₹{order.Amount}", bodyFont));
            document.Add(new Paragraph($"Payment Status: {order.PaymentStatus}", bodyFont));
            document.Add(new Paragraph($"Customer Email: {order.UserEmail}", bodyFont));

            document.Close();

            return fullPath;
        }

        // Sends the invoice PDF to the customer via email.
        private void SendInvoiceEmail(string customerEmail, string invoicePath)
        {
            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Invoices", Path.GetFileName(invoicePath));

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(smtpEmail);
            mail.To.Add(customerEmail);
            mail.Subject = "Your Invoice";
            mail.Body = "Thank you for your purchase. Please find your invoice attached.";
            mail.Attachments.Add(new Attachment(absolutePath));

            SmtpClient smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpEmail, smtpPassword)
            };

            smtp.Send(mail);
        }
        public IActionResult EmployeeOrders()
        {
            var orders = db.Orders.ToList();

            // Attach Product Name to each Order (without using a ViewModel)
            foreach (var order in orders)
            {
                var product = db.Products.FirstOrDefault(p => p.Id == order.ProductId);
                if (product != null)
                {
                    order.UserEmail += $" ({product.Name})"; // Temporary way to display Product Name
                }
            }

            return View(orders);
        }



        public IActionResult EditStatus(int orderId)
        {
            var order = db.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: /Orders/EditStatus/{orderId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStatus(int orderId, string orderStatus)
        {
            var order = db.Orders.FirstOrDefault(o => o.Id == orderId);
            

            order.OrderStatus = orderStatus;
            db.SaveChanges();

            return RedirectToAction("EmployeeOrders"); // or wherever you list orders
        }

        // Example action to list orders for the employee
        public IActionResult EmployeeProducts()
        {
            // Get the logged-in employee's email from session
            string employeeEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(employeeEmail))
            {
                return RedirectToAction("Account", "Account"); // Redirect if not logged in
            }

            // Fetch only products added by this employee
            var products = db.Products.ToList();

            return View(products);
        }


    }
}
