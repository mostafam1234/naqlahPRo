namespace Application.Features.AdminSection.Dashboard.Dtos
{
    public class DashboardStatisticsDto
    {
        public int TotalOrders { get; set; }
        public int TodayOrders { get; set; }
        public int ThisWeekOrders { get; set; }
        public int TotalCustomers { get; set; }
    }
}

