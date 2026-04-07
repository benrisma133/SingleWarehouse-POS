using Microsoft.Data.Sqlite;
using System;
using System.Data;

namespace POS_DAL
{
    public class clsSaleDetailsData
    {
        // All sales (header rows – one per sale)
        public static DataTable GetAllSales(string filter = "")
        {
            string sql = @"
        SELECT SaleID, SaleDate, ClientName, ClientPhone, ClientEmail,
               SUM(LineTotal) AS TotalPrice, COUNT(DetailID) AS ItemCount
        FROM   vw_SaleDetails
        WHERE  ClientName LIKE @f OR CAST(SaleID AS TEXT) LIKE @f
        GROUP  BY SaleID, SaleDate, ClientName, ClientPhone, ClientEmail
        ORDER  BY SaleDate DESC";

            var dt = new DataTable();

            using (var cn = DbHelper.OpenConnection())
            using (var cmd = new SqliteCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@f",
                    string.IsNullOrEmpty(filter) ? "%" : $"%{filter}%");

                cn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }

        // Detail lines for one sale
        public static DataTable GetSaleItems(int saleID)
        {
            string sql = @"
        SELECT ProductName, BrandName, SeriesName, ModelName,
               CategoryName, Quantity, UnitPrice, LineTotal,
               ProductDescription
        FROM   vw_SaleDetails
        WHERE  SaleID = @id";

            var dt = new DataTable();

            using (var cn = DbHelper.OpenConnection())
            using (var cmd = new SqliteCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@id", saleID);

                cn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }

        public static bool DeleteSale(int saleID)
        {
            using (var cn = DbHelper.OpenConnection())
            {
                cn.Open();

                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        Exec(cn, tx, "DELETE FROM SalesDetails WHERE SaleID=@id", saleID);
                        Exec(cn, tx, "DELETE FROM Sales WHERE SaleID=@id", saleID);

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        return false;
                    }
                }
            }
        }

        private static void Exec(SqliteConnection cn, SqliteTransaction tx, string sql, int id)
        {
            using (var cmd = new SqliteCommand(sql, cn, tx))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}