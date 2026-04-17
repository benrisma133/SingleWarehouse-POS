using Microsoft.Data.Sqlite;
using POS_DAL.Loggers;
using System;
using System.Data;

namespace POS_DAL
{
    public class clsSaleDetailsData
    {
        private const string _className = nameof(clsSaleDetailsData);

        // ============================
        // GET ALL SALES
        // ============================
        public static DataTable GetAllSales(string filter = "")
        {
            string sql = @"
                SELECT SaleID, SaleDate, ClientName, ClientPhone, ClientEmail,
                       SUM(LineTotal) AS TotalPrice, COUNT(DetailID) AS ItemCount
                FROM vw_SaleDetails
                WHERE ClientName LIKE @f OR CAST(SaleID AS TEXT) LIKE @f
                GROUP BY SaleID, SaleDate, ClientName, ClientPhone, ClientEmail
                ORDER BY SaleDate DESC";

            var dt = new DataTable();

            try
            {
                using (var cn = DbHelper.OpenConnection())
                using (var cmd = new SqliteCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@f",
                        string.IsNullOrEmpty(filter) ? "%" : $"%{filter}%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetAllSales), ex);
            }

            return dt;
        }

        // ============================
        // GET SALE ITEMS
        // ============================
        public static DataTable GetSaleItems(int saleID)
        {
            string sql = @"
                SELECT ProductName, BrandName, SeriesName, ModelName,
                       CategoryName, Quantity, UnitPrice, LineTotal,
                       ProductDescription
                FROM vw_SaleDetails
                WHERE SaleID = @id";

            var dt = new DataTable();

            try
            {
                using (var cn = DbHelper.OpenConnection())
                using (var cmd = new SqliteCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@id", saleID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetSaleItems), ex);
            }

            return dt;
        }

        // ============================
        // DELETE SALE
        // ============================
        public static bool DeleteSale(int saleID)
        {
            try
            {
                using (var cn = DbHelper.OpenConnection())
                using (var tx = cn.BeginTransaction())
                {
                    Exec(cn, tx, "DELETE FROM SalesDetails WHERE SaleID = @id", saleID);
                    Exec(cn, tx, "DELETE FROM Sales WHERE SaleID = @id", saleID);

                    tx.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(DeleteSale), ex);
                return false;
            }
        }

        // ============================
        // HELPER METHOD
        // ============================
        private static void Exec(SqliteConnection cn, SqliteTransaction tx, string sql, int id)
        {
            using (var cmd = new SqliteCommand(sql, cn, tx))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        // ============================
        // GET SALE BY ID
        // ============================
        public static bool GetByID(int saleID, ref int clientID, ref DateTime saleDate, ref decimal totalPrice)
        {
            try
            {
                using (var cn = DbHelper.OpenConnection())
                using (var cmd = new SqliteCommand(@"
                    SELECT ClientID, SaleDate, SUM(LineTotal) AS TotalPrice
                    FROM vw_SaleDetails
                    WHERE SaleID = @SaleID
                    GROUP BY SaleID", cn))
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
        // GET SALE ITEM COUNT
        // ============================
        public static int GetSaleItemCount(int saleID)
        {
            try
            {
                using (var cn = DbHelper.OpenConnection())
                using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM SalesDetails WHERE SaleID = @SaleID", cn))
                {
                    cmd.Parameters.AddWithValue("@SaleID", saleID);
                    object result = cmd.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetSaleItemCount), ex);
                return 0;
            }
        }

        // ============================
        // GET TOTAL SALES AMOUNT
        // ============================
        public static decimal GetTotalSalesAmount()
        {
            try
            {
                using (var cn = DbHelper.OpenConnection())
                using (var cmd = new SqliteCommand("SELECT IFNULL(SUM(LineTotal), 0) FROM vw_SaleDetails", cn))
                {
                    object result = cmd.ExecuteScalar();
                    return result == null ? 0 : Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetTotalSalesAmount), ex);
                return 0;
            }
        }
    }
}