using Microsoft.Data.Sqlite;
using POS_DAL.Loggers;
using System;
using System.Collections.Generic;
using System.Data;

namespace POS_DAL
{
    public static class clsSalesData
    {
        private const string _className = nameof(clsSalesData);

        // ── Insert sale header + all detail lines in one transaction ─────────────
        // Returns new SaleID, or -1 on failure.
        // Throws InvalidOperationException if any item is out of stock.
        public static int AddSale(int clientID, decimal totalPrice,
                                   IEnumerable<(int ProductID, string ProductName,
                                                int Quantity, decimal UnitPrice)> items)
        {
            try
            {
                using (var conn = DbHelper.OpenConnection())
                using (var tr = conn.BeginTransaction())
                {
                    // 1. Sale header
                    int newSaleID;
                    using (var cmd = new SqliteCommand(@"
                        INSERT INTO Sales (ClientID, SaleDate, TotalPrice)
                        VALUES (@ClientID, datetime('now','localtime'), @TotalPrice);
                        SELECT last_insert_rowid();", conn, tr))
                    {
                        cmd.Parameters.AddWithValue("@ClientID", clientID);
                        cmd.Parameters.AddWithValue("@TotalPrice", totalPrice);
                        object result = cmd.ExecuteScalar();
                        if (result == null) { tr.Rollback(); return -1; }
                        newSaleID = Convert.ToInt32((long)result);
                    }

                    // 2. Detail lines + stock deduction
                    foreach (var item in items)
                    {
                        // Stock check
                        int currentStock;
                        using (var cmd = new SqliteCommand(
                            "SELECT Quantity FROM Products WHERE ProductID = @PID;", conn, tr))
                        {
                            cmd.Parameters.AddWithValue("@PID", item.ProductID);
                            object r = cmd.ExecuteScalar();
                            currentStock = r != null ? Convert.ToInt32(r) : 0;
                        }

                        if (currentStock < item.Quantity)
                        {
                            tr.Rollback();
                            throw new InvalidOperationException(
                                $"Insufficient stock for '{item.ProductName}'. " +
                                $"Available: {currentStock}, Requested: {item.Quantity}");
                        }

                        using (var cmd = new SqliteCommand(@"
                            INSERT INTO SalesDetails (SaleID, ProductID, Quantity, Price)
                            VALUES (@SaleID, @ProductID, @Quantity, @Price);", conn, tr))
                        {
                            cmd.Parameters.AddWithValue("@SaleID", newSaleID);
                            cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@Price", item.UnitPrice);
                            if (cmd.ExecuteNonQuery() == 0) { tr.Rollback(); return -1; }
                        }

                        using (var cmd = new SqliteCommand(@"
                            UPDATE Products
                            SET Quantity = Quantity - @Qty
                            WHERE ProductID = @PID;", conn, tr))
                        {
                            cmd.Parameters.AddWithValue("@Qty", item.Quantity);
                            cmd.Parameters.AddWithValue("@PID", item.ProductID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tr.Commit();
                    return newSaleID;
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(AddSale), ex);
                return -1;
            }
        }

        // ── Delete sale, restore stock ────────────────────────────────────────────
        public static bool Delete(int saleID)
        {
            try
            {
                using (var conn = DbHelper.OpenConnection())
                using (var tr = conn.BeginTransaction())
                {
                    // Restore stock row by row (SQLite has no UPDATE…FROM)
                    using (var cmd = new SqliteCommand(
                        "SELECT ProductID, Quantity FROM SalesDetails WHERE SaleID = @SaleID;",
                        conn, tr))
                    {
                        cmd.Parameters.AddWithValue("@SaleID", saleID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                using (var upd = new SqliteCommand(
                                    "UPDATE Products SET Quantity = Quantity + @Qty WHERE ProductID = @PID;",
                                    conn, tr))
                                {
                                    upd.Parameters.AddWithValue("@Qty", reader.GetInt32(1));
                                    upd.Parameters.AddWithValue("@PID", reader.GetInt32(0));
                                    upd.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    using (var cmd = new SqliteCommand(
                        "DELETE FROM SalesDetails WHERE SaleID = @SaleID;", conn, tr))
                    {
                        cmd.Parameters.AddWithValue("@SaleID", saleID);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqliteCommand(
                        "DELETE FROM Sales WHERE SaleID = @SaleID;", conn, tr))
                    {
                        cmd.Parameters.AddWithValue("@SaleID", saleID);
                        cmd.ExecuteNonQuery();
                    }

                    tr.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Delete), ex);
                return false;
            }
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public static DataTable GetAll()
        {
            var dt = new DataTable();
            try
            {
                using (var conn = DbHelper.OpenConnection())
                using (var cmd = new SqliteCommand(@"
                    SELECT s.SaleID,
                           c.FirstName || ' ' || c.LastName AS ClientName,
                           s.SaleDate,
                           s.TotalPrice
                    FROM Sales s
                    JOIN Clients c ON c.ClientID = s.ClientID
                    ORDER BY s.SaleDate DESC;", conn))
                using (var reader = cmd.ExecuteReader())
                    dt.Load(reader);
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetAll), ex);
            }
            return dt;
        }

        public static DataTable GetSaleDetails(int saleID)
        {
            var dt = new DataTable();
            try
            {
                using (var conn = DbHelper.OpenConnection())
                using (var cmd = new SqliteCommand(@"
                    SELECT sd.DetailID, p.ProductName, sd.Quantity, sd.Price,
                           sd.Quantity * sd.Price AS LineTotal
                    FROM SalesDetails sd
                    JOIN Products p ON p.ProductID = sd.ProductID
                    WHERE sd.SaleID = @SaleID;", conn))
                {
                    cmd.Parameters.AddWithValue("@SaleID", saleID);
                    using (var reader = cmd.ExecuteReader())
                        dt.Load(reader);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetSaleDetails), ex);
            }
            return dt;
        }

        public static DataTable GetAvailableProducts(string filter = "")
        {
            var dt = new DataTable();
            try
            {
                using (var conn = DbHelper.OpenConnection())
                using (var cmd = new SqliteCommand(@"
                    SELECT ProductID, ProductName, Price, Quantity
                    FROM Products
                    WHERE Quantity > 0
                      AND (ProductName LIKE @Filter OR CAST(Price AS TEXT) LIKE @Filter)
                    ORDER BY ProductName;", conn))
                {
                    cmd.Parameters.AddWithValue("@Filter", $"%{filter}%");
                    using (var reader = cmd.ExecuteReader())
                        dt.Load(reader);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetAvailableProducts), ex);
            }
            return dt;
        }

        public static DataTable GetClients()
        {
            var dt = new DataTable();
            try
            {
                using (var conn = DbHelper.OpenConnection())
                using (var cmd = new SqliteCommand(@"
                    SELECT ClientID,
                           FirstName || ' ' || LastName AS FullName,
                           Phone
                    FROM Clients
                    ORDER BY FirstName;", conn))
                using (var reader = cmd.ExecuteReader())
                    dt.Load(reader);
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetClients), ex);
            }
            return dt;
        }

        public static int GetProductStock(int productID)
        {
            try
            {
                using (var conn = DbHelper.OpenConnection())
                using (var cmd = new SqliteCommand(
                    "SELECT Quantity FROM Products WHERE ProductID = @PID;", conn))
                {
                    cmd.Parameters.AddWithValue("@PID", productID);
                    object r = cmd.ExecuteScalar();
                    return r != null ? Convert.ToInt32(r) : 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetProductStock), ex);
                return 0;
            }
        }

        // ============================
        // GET SALE BY ID
        // ============================
        public static bool GetByID(int saleID, ref int clientID, ref DateTime saleDate, ref decimal totalPrice)
        {
            try
            {
                using (var conn = DbHelper.OpenConnection())
                using (var cmd = new SqliteCommand(@"
                    SELECT ClientID, SaleDate, TotalPrice
                    FROM Sales
                    WHERE SaleID = @SaleID;", conn))
                {
                    cmd.Parameters.AddWithValue("@SaleID", saleID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            clientID = reader.GetInt32(0);
                            saleDate = reader.IsDBNull(1) ? DateTime.Now : Convert.ToDateTime(reader.GetString(1));
                            totalPrice = reader.GetDecimal(2);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetByID), ex);
            }
            return false;
        }

        // ============================
        // GET TOTAL SALES COUNT
        // ============================
        public static int GetTotalSalesCount()
        {
            try
            {
                using (var conn = DbHelper.OpenConnection())
                using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM Sales;", conn))
                {
                    object result = cmd.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetTotalSalesCount), ex);
                return 0;
            }
        }

        // ============================
        // GET TOTAL REVENUE
        // ============================
        public static decimal GetTotalRevenue()
        {
            try
            {
                using (var conn = DbHelper.OpenConnection())
                using (var cmd = new SqliteCommand("SELECT IFNULL(SUM(TotalPrice), 0) FROM Sales;", conn))
                {
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
    }
}