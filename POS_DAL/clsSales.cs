using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;

namespace POS_DAL
{
    public static class clsSalesData
    {
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
                        newSaleID = Convert.ToInt32(result);
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
                            SET    Quantity = Quantity - @Qty
                            WHERE  ProductID = @PID;", conn, tr))
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
            catch (InvalidOperationException) { throw; }
            catch { return -1; }
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
            catch { return false; }
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
                    FROM   Sales s
                    JOIN   Clients c ON c.ClientID = s.ClientID
                    ORDER  BY s.SaleDate DESC;", conn))
                using (var reader = cmd.ExecuteReader())
                    dt.Load(reader);
            }
            catch { }
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
                    FROM   SalesDetails sd
                    JOIN   Products p ON p.ProductID = sd.ProductID
                    WHERE  sd.SaleID = @SaleID;", conn))
                {
                    cmd.Parameters.AddWithValue("@SaleID", saleID);
                    using (var reader = cmd.ExecuteReader())
                        dt.Load(reader);
                }
            }
            catch { }
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
                    FROM   Products
                    WHERE  Quantity > 0
                      AND (ProductName LIKE @Filter OR CAST(Price AS TEXT) LIKE @Filter)
                    ORDER  BY ProductName;", conn))
                {
                    cmd.Parameters.AddWithValue("@Filter", $"%{filter}%");
                    using (var reader = cmd.ExecuteReader())
                        dt.Load(reader);
                }
            }
            catch { }
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
                    FROM   Clients
                    ORDER  BY FirstName;", conn))
                using (var reader = cmd.ExecuteReader())
                    dt.Load(reader);
            }
            catch { }
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
            catch { return 0; }
        }
    }
}