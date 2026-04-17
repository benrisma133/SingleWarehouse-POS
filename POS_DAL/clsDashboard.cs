using Microsoft.Data.Sqlite;
using POS_DAL.Loggers;
using System;
using System.Data;

namespace POS_DAL
{
    public static class clsDashboardData
    {
        private const string _className = nameof(clsDashboardData);

        public static int GetTotalProducts()
        {
            try
            {
                using (var con = DbHelper.OpenConnection())
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Products";
                    object result = cmd.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetTotalProducts), ex);
                return 0;
            }
        }

        public static int GetTotalSales()
        {
            try
            {
                using (var con = DbHelper.OpenConnection())
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Sales";
                    object result = cmd.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetTotalSales), ex);
                return 0;
            }
        }

        public static decimal GetTotalRevenue()
        {
            try
            {
                using (var con = DbHelper.OpenConnection())
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT IFNULL(SUM(TotalPrice),0) FROM Sales";
                    object result = cmd.ExecuteScalar();
                    return result == null ? 0 : Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetTotalRevenue), ex);
                return 0;
            }
        }

        public static int GetLowStockCount(int threshold)
        {
            try
            {
                using (var con = DbHelper.OpenConnection())
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Products WHERE Quantity <= @t";
                    cmd.Parameters.AddWithValue("@t", threshold);

                    object result = cmd.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetLowStockCount), ex);
                return 0;
            }
        }

        public static DataTable GetSalesLast7Days()
        {
            try
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
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetSalesLast7Days), ex);
                return new DataTable();
            }
        }

        public static DataTable GetLowStockItems(int threshold)
        {
            try
            {
                using (var con = DbHelper.OpenConnection())
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT ProductName, Quantity
                        FROM Products
                        WHERE Quantity <= @t
                        ORDER BY Quantity ASC
                        LIMIT 20
                    ";
                    cmd.Parameters.AddWithValue("@t", threshold);

                    using (var reader = cmd.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetLowStockItems), ex);
                return new DataTable();
            }
        }

        public static DataTable GetRecentSales(int limit)
        {
            try
            {
                using (var con = DbHelper.OpenConnection())
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT s.SaleID,
                               (c.FirstName || ' ' || c.LastName) AS Client,
                               s.SaleDate,
                               s.TotalPrice
                        FROM Sales s
                        JOIN Clients c ON c.ClientID = s.ClientID
                        ORDER BY s.SaleDate DESC
                        LIMIT @lim
                    ";
                    cmd.Parameters.AddWithValue("@lim", limit);

                    using (var reader = cmd.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetRecentSales), ex);
                return new DataTable();
            }
        }
    }
}