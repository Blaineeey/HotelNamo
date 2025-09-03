using System.Collections.Generic;

namespace HotelNamo.Models
{
    public class AdminDashboardViewModel
    {
        // User Statistics
        public int TotalUsers { get; set; }
        public int TotalStaff { get; set; }
        public int TotalAdmins { get; set; }

        // Room Statistics
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int MaintenanceRooms { get; set; }

        // Booking Statistics
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }

        // Financial Statistics
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }

        // Recent Activities
        public List<Booking> RecentBookings { get; set; } = new List<Booking>();
        public List<Payment> RecentPayments { get; set; } = new List<Payment>();
        public List<MaintenanceRequest> PendingMaintenance { get; set; } = new List<MaintenanceRequest>();
        public List<HousekeepingTask> PendingHousekeeping { get; set; } = new List<HousekeepingTask>();

        // Chart Data
        public List<ChartData> MonthlyBookings { get; set; } = new List<ChartData>();
        public List<ChartData> RoomTypeDistribution { get; set; } = new List<ChartData>();
        public List<ChartData> RevenueByMonth { get; set; } = new List<ChartData>();
    }

    public class ChartData
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
