using Microsoft.AspNetCore.Mvc;
using OpticalShopWebsite.Data;
using OpticalShopWebsite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;

namespace OpticalShopWebsite.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext db;

        // SMTP credentials for sending payslip
        private readonly string smtpEmail = "onkyasalunkhe@gmail.com";
        private readonly string smtpPassword = "bddhfhgjscxjlxpk";

        public AdminController(ApplicationDbContext db)
        {
            this.db = db;
        }

        // GET: /Admin/AddSalary
        public IActionResult AddSalary()
        {
            // Fetch all employees (Users where Role = "Employee")
            var employees = db.Users.Where(u => u.Role == "Employee").ToList();
            ViewBag.Employees = employees;
            return View();
        }

        // POST: /Admin/AddSalary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddSalary(string employeeEmail, decimal amount, string month, int year)
        {
            if (string.IsNullOrEmpty(employeeEmail) || amount <= 0 || string.IsNullOrEmpty(month) || year <= 0)
            {
                return BadRequest("Invalid salary details.");
            }

            // Create Salary record
            Salary salary = new Salary
            {
                EmployeeEmail = employeeEmail,
                Amount = amount,
                Month = month,
                Year = year
            };
            string payslipPath = GeneratePayslipPdf(salary);
            salary.PayslipPath = payslipPath;
            db.Salaries.Add(salary);
            db.SaveChanges();

            // Generate Payslip
            
            

            // Send Payslip to Employee
            SendPayslipEmail(employeeEmail, payslipPath);

            return RedirectToAction("AddSalary");
        }
        public IActionResult MyPayslips()
        {
            string employeeEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(employeeEmail))
            {
                return RedirectToAction("Account", "Account");
            }

            var payslips = db.Salaries.Where(s => s.EmployeeEmail == employeeEmail).OrderByDescending(s => s.DateGenerated).ToList();
            return View(payslips);
        }

        // Generates Payslip PDF using iTextSharp
        private string GeneratePayslipPdf(Salary salary)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Payslips");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = $"Payslip_{salary.EmployeeEmail}_{salary.Month}_{salary.Year}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);

            Document document = new Document(PageSize.A4);
            PdfWriter.GetInstance(document, new FileStream(fullPath, FileMode.Create));
            document.Open();

            Font titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD);
            Paragraph title = new Paragraph("Payslip", titleFont) { Alignment = Element.ALIGN_CENTER };
            document.Add(title);
            document.Add(new Paragraph("\n"));

            Font bodyFont = FontFactory.GetFont("Arial", 12, Font.NORMAL);
            document.Add(new Paragraph($"Employee Email: {salary.EmployeeEmail}", bodyFont));
            document.Add(new Paragraph($"Month: {salary.Month}", bodyFont));
            document.Add(new Paragraph($"Year: {salary.Year}", bodyFont));
            document.Add(new Paragraph($"Salary Amount: ₹{salary.Amount}", bodyFont));
            document.Add(new Paragraph($"Generated On: {salary.DateGenerated:dd MMM yyyy}", bodyFont));

            document.Close();

            return fullPath;
        }

        // Sends Payslip to Employee via Email
        private void SendPayslipEmail(string employeeEmail, string payslipPath)
        {
            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Payslips", Path.GetFileName(payslipPath));

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(smtpEmail);
            mail.To.Add(employeeEmail);
            mail.Subject = "Your Payslip";
            mail.Body = "Your payslip for this month is attached.";
            mail.Attachments.Add(new Attachment(absolutePath));

            SmtpClient smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpEmail, smtpPassword)
            };

            smtp.Send(mail);
        }

        // Admin View to List Generated Payslips
        public IActionResult EmployeePayslips()
        {
            var payslips = db.Salaries.OrderByDescending(s => s.DateGenerated).ToList();
            return View(payslips);
        }
    }
}
