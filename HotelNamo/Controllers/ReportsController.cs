using HotelNamo.Data;
using HotelNamo.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelectPdf;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelNamo.Controllers
{
    [Authorize(Roles = "Admin")]

    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports/Bookings
        public async Task<IActionResult> BookingsReport()
        {
            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return View(bookings);
        }
        public IActionResult ExportBookingsReport()
        {
            var bookings = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .OrderByDescending(b => b.CreatedDate)
                .ToList();

            string htmlContent = "<h2>Booking Report</h2><table border='1'><tr><th>Booking ID</th><th>User</th><th>Room</th><th>Check-in</th><th>Check-out</th><th>Total Price</th><th>Status</th></tr>";

            foreach (var booking in bookings)
            {
                htmlContent += $"<tr><td>{booking.Id}</td><td>{booking.User?.Email}</td><td>{booking.Room?.RoomNumber} ({booking.Room?.Category})</td><td>{booking.CheckInDate.ToShortDateString()}</td><td>{booking.CheckOutDate.ToShortDateString()}</td><td>{booking.TotalPrice:C}</td><td>{(booking.IsConfirmed ? "Confirmed" : "Pending")}</td></tr>";
            }

            htmlContent += "</table>";

            HtmlToPdf converter = new HtmlToPdf();
            PdfDocument pdf = converter.ConvertHtmlString(htmlContent);
            byte[] pdfBytes = pdf.Save();

            return File(pdfBytes, "application/pdf", "BookingsReport.pdf");
        }
        // GET: Reports/Revenue
        public IActionResult RevenueReport()
        {
            var revenueData = _context.Bookings
                .Where(b => b.IsConfirmed)
                .GroupBy(b => new { b.CheckInDate.Year, b.CheckInDate.Month }) // Group by Year & Month
                .Select(g => new RevenueReportViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(b => b.TotalPrice)
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month)
                .ToList();

            return View(revenueData);
        }

        // GET: Reports/Occupancy
        public async Task<IActionResult> OccupancyReport()
        {
            var occupancyData = await _context.Rooms
                .Select(r => new OccupancyReportViewModel
                {
                    RoomNumber = r.RoomNumber,
                    TimesBooked = _context.Bookings.Count(b => b.RoomId == r.Id && b.IsConfirmed) // Only count confirmed bookings
                })
                .OrderByDescending(r => r.TimesBooked)
                .ToListAsync();

            return View(occupancyData);
        }
    }
}
