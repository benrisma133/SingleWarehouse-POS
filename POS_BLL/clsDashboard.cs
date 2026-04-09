using System.Data;
using POS_DAL;

namespace POS_BLL
{
    public static class clsDashboard
    {
        public static int GetTotalProducts()
            => clsDashboardData.GetTotalProducts();

        public static int GetTotalSales()
            => clsDashboardData.GetTotalSales();

        public static decimal GetRevenue()
            => clsDashboardData.GetTotalRevenue();

        public static int GetLowStock(int threshold = 10)
            => clsDashboardData.GetLowStockCount(threshold);

        public static DataTable GetSalesChart()
            => clsDashboardData.GetSalesLast7Days();

        // Add to clsDashboard

        public static DataTable GetLowStockItems(int threshold = 10)
            => clsDashboardData.GetLowStockItems(threshold);

        public static DataTable GetRecentSales(int limit = 10)
            => clsDashboardData.GetRecentSales(limit);
    }
}