using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace Project_64140855.Models
{
    public class Dashboard_64130855
    {

        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalGames { get; set; }
        public int TotalUsers { get; set; }

        // Dữ liệu biểu đồ: thống kê số đơn hàng theo ngày
        public List<OrderByDate> OrdersByDate { get; set; }

        // Dữ liệu biểu đồ: số đơn hàng theo trạng thái
        public List<OrderStatusGroup> OrderStatusGroups { get; set; }
    }

    public class OrderByDate
    {
        public string Date { get; set; } // Ví dụ "19/05"
        public int Count { get; set; }
    }

    public class OrderStatusGroup
    {
        public string Status { get; set; } // Ví dụ "Đã thanh toán"
        public int Count { get; set; }
    }
}
