using Microsoft.Data.Sqlite;
using System;
using System.Data;

namespace POS_DAL
{
    public static class clsDashboardData
    {
        public static int GetTotalProducts()
        {
            using (var con = DbHelper.OpenConnection())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Products";
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static int GetTotalSales()
        {
            using (var con = DbHelper.OpenConnection())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Sales";
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static decimal GetTotalRevenue()
        {
            using (var con = DbHelper.OpenConnection())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT IFNULL(SUM(TotalPrice),0) FROM Sales";
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        public static int GetLowStockCount(int threshold)
        {
            using (var con = DbHelper.OpenConnection())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Products WHERE Quantity <= @t";
                cmd.Parameters.AddWithValue("@t", threshold);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static DataTable GetSalesLast7Days()
        {
            using (var con = DbHelper.OpenConnection())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT DATE(SaleDate) as Day, SUM(TotalPrice) as Total
                    FROM Sales
                    GROUP BY DATE(SaleDate)
                    ORDER BY Day DESC
                    LIMIT 7
                ";

                using (var reader = cmd.ExecuteReader())
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
            }
        }

        // Add inside clsDashboardData — alongside existing methods

        public static DataTable GetLowStockItems(int threshold)
        {
            using (var con = DbHelper.OpenConnection())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
            SELECT ProductName, Quantity
            FROM   Products
            WHERE  Quantity <= @t
            ORDER  BY Quantity ASC
            LIMIT  20";
                cmd.Parameters.AddWithValue("@t", threshold);
                using (var reader = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
            }
        }

        public static DataTable GetRecentSales(int limit)
        {
            using (var con = DbHelper.OpenConnection())
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
            SELECT s.SaleID,
                   (c.FirstName || ' ' || c.LastName) AS Client,
                   s.SaleDate,
                   s.TotalPrice
            FROM   Sales    s
            JOIN   Clients  c ON c.ClientID = s.ClientID
            ORDER  BY s.SaleDate DESC
            LIMIT  @lim";
                cmd.Parameters.AddWithValue("@lim", limit);
                using (var reader = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
            }
        }
    }
}