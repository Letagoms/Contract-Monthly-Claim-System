using Microsoft.AspNetCore.Mvc;
using Contract_Monthly_Claim_System.Data;
using Contract_Monthly_Claim_System.Filters;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Contract_Monthly_Claim_System.Controllers
{
    [SessionAuthorization]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
            
            // Set QuestPDF license (Community license is free)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, bool allTime = false)
        {
            // Query for approved claims only (both Coordinator and Manager approved)
            var query = _context.Claims
                .Include(c => c.User)
                .Where(c => c.CoordinatorStatus == "Approved" && c.ManagerStatus == "Approved");

            // Apply date filtering if not "all time"
            if (!allTime && startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(c => c.SubmittedDate >= startDate.Value && c.SubmittedDate <= endDate.Value);
            }

            var approvedClaims = await query
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();

            // Calculate totals
            var totalClaims = approvedClaims.Count;
            var totalAmount = approvedClaims.Sum(c => c.TotalAmount);

            ViewBag.TotalClaims = totalClaims;
            ViewBag.TotalAmount = totalAmount;
            ViewBag.ApprovedClaims = approvedClaims;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.AllTime = allTime;

            return View();
        }

        public async Task<IActionResult> ExportReport(DateTime? startDate, DateTime? endDate, bool allTime = false)
        {
            try
            {
                // Query for approved claims
                var query = _context.Claims
                    .Include(c => c.User)
                    .Where(c => c.CoordinatorStatus == "Approved" && c.ManagerStatus == "Approved");

                if (!allTime && startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(c => c.SubmittedDate >= startDate.Value && c.SubmittedDate <= endDate.Value);
                }

                var approvedClaims = await query
                    .OrderByDescending(c => c.SubmittedDate)
                    .ToListAsync();

                // Calculate totals
                var totalClaims = approvedClaims.Count;
                var totalAmount = approvedClaims.Sum(c => c.TotalAmount);

                // Generate PDF
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // Header
                        page.Header().Element(ComposeHeader);

                        // Content
                        page.Content().Column(column =>
                        {
                            column.Spacing(10);

                            // Report Info
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Report Information").Bold().FontSize(14);
                                    col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                                    
                                    if (allTime)
                                    {
                                        col.Item().Text("Period: All Time");
                                    }
                                    else if (startDate.HasValue && endDate.HasValue)
                                    {
                                        col.Item().Text($"Period: {startDate.Value:yyyy-MM-dd} to {endDate.Value:yyyy-MM-dd}");
                                    }
                                });

                                row.RelativeItem().AlignRight().Column(col =>
                                {
                                    col.Item().Text($"Total Claims: {totalClaims}").Bold();
                                    col.Item().Text($"Total Amount: R{totalAmount:N2}").Bold().FontSize(12);
                                });
                            });

                            // Space
                            column.Item().PaddingVertical(5);

                            // Table
                            column.Item().Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Lecturer Name
                                    columns.RelativeColumn(2); // Course
                                    columns.RelativeColumn(1.5f); // Lecture Date
                                    columns.RelativeColumn(1); // Hours
                                    columns.RelativeColumn(1); // Rate
                                    columns.RelativeColumn(1.5f); // Total
                                    columns.RelativeColumn(1.5f); // Submitted
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Lecturer").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Course").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Lecture Date").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Hours").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Rate").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Total").Bold();
                                    header.Cell().Background(Colors.Blue.Lighten3).Padding(5).Text("Submitted").Bold();
                                });

                                // Rows
                                foreach (var claim in approvedClaims)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.LecturerName);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.CourseName);
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.LectureDate.ToString("yyyy-MM-dd"));
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.HoursWorked.ToString("N1"));
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"R{claim.HourlyRate:N2}");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"R{claim.TotalAmount:N2}");
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.SubmittedDate.ToString("yyyy-MM-dd"));
                                }
                            });
                        });

                        // Footer
                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                    });
                });

                void ComposeHeader(IContainer container)
                {
                    container.Row(row =>
                    {
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text("HR Payment Report").Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                            column.Item().Text("Approved Claims for Payment Processing").FontSize(12);
                        });
                    });
                }

                // Generate PDF bytes
                var pdfBytes = document.GeneratePdf();

                // Generate filename
                var fileName = $"HR_Payment_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                _logger.LogInformation($"PDF Report exported: {totalClaims} claims, Total: R{totalAmount:N2}");

                // Return PDF file
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting PDF report");
                TempData["ErrorMessage"] = "Failed to export report. Please try again.";
                return RedirectToAction("Index", new { startDate, endDate, allTime });
            }
        }
    }
}