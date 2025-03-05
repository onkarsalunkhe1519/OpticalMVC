using Microsoft.AspNetCore.Mvc;
using OpticalShopWebsite.Data;
using OpticalShopWebsite.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;
using System.Net;
using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace OpticalShopWebsite.Controllers
{
    public class EyeReportController : Controller
    {
        private readonly ApplicationDbContext db;
        public EyeReportController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            var reports = db.EyeReports.ToList();
            return View(reports);
        }
        // GET: /EyeReport/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /EyeReport/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EyeReport report)
        {
            
                // Save the report in the database
                db.EyeReports.Add(report);
                db.SaveChanges();

                // Generate PDF file for the report
                string pdfPath = GenerateEyeReportPdf(report);

                // Send email with the PDF attached
                SendEmailWithPdf(report.CustomerEmail, pdfPath);

                return RedirectToAction("Success");
            
           
        }

        // GET: /EyeReport/Success
        public IActionResult Success()
        {
            return View();
        }

        /// <summary>
        /// Generates a PDF for the eye report and returns the file path.
        /// </summary>
        private string GenerateEyeReportPdf(EyeReport report)
        {
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Reports");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"EyeReport_{report.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);

            iTextSharp.text.Document document = new iTextSharp.text.Document(PageSize.A4);
            PdfWriter.GetInstance(document, new FileStream(fullPath, FileMode.Create));
            document.Open();

            // Title
            iTextSharp.text.Font titleFont = FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD);
            Paragraph title = new Paragraph("Eye Report", titleFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            document.Add(title);
            document.Add(new Chunk("\n"));

            // Body
            iTextSharp.text.Font bodyFont = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL);
            document.Add(new Paragraph($"Customer Email: {report.CustomerEmail}", bodyFont));
            document.Add(new Paragraph($"Report Date: {report.ReportDate:dd MMM yyyy}", bodyFont));
            document.Add(new Paragraph($"Left Cylinder: {report.LeftCylinder}", bodyFont));
            document.Add(new Paragraph($"Right Cylinder: {report.RightCylinder}", bodyFont));

            document.Close();

            return fullPath;
        }

        /// <summary>
        /// Sends an email with the PDF file attached.
        /// </summary>
        private void SendEmailWithPdf(string customerEmail, string pdfPath)
        {
            string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Reports", Path.GetFileName(pdfPath));

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("onkyasalunkhe@gmail.com");
            mail.To.Add(customerEmail);
            mail.Subject = "Your Eye Report";
            mail.Body = "Please find attached your eye report PDF.";
            mail.Attachments.Add(new Attachment(absolutePath));

            SmtpClient smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential("onkyasalunkhe@gmail.com", "bddhfhgjscxjlxpk")
            };

            smtp.Send(mail);
        }
    }
}
