using System;
using System.Data;
using Microsoft.Data.Sqlite;
using POS_DAL.Loggers;

namespace POS_DAL
{
    public static class clsProductData
    {
        private const string _className = nameof(clsProductData);

        // ============================
        // GET PRODUCT BY ID
        // ============================
        public static bool GetProduct(
            int productID,
            ref int categoryID, ref int modelID, ref decimal price,
            ref string productName, ref string description,
            ref int quantity)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            CategoryID,
                            ModelID,
                            Price,
                            ProductName,
                            Description,
                            Quantity
                        FROM Products
                        WHERE ProductID = @ProductID;
                    ";

                    command.Parameters.AddWithValue("@ProductID", productID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            categoryID = reader.IsDBNull(0) ? -1 : reader.GetInt32(0);
                            modelID = reader.IsDBNull(1) ? -1 : reader.GetInt32(1);
                            price = reader.GetDecimal(2);
                            productName = reader.GetString(3);
                            description = reader.IsDBNull(4) ? null : reader.GetString(4);
                            quantity = reader.GetInt32(5);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetProduct), ex);
                throw new Exception("Error in GetProduct: " + ex.Message);
            }

            return false;
        }

        // ============================
        // ADD NEW PRODUCT
        // ============================
        public static int AddNew(
            string productName, string description, decimal price,
            int? categoryID, int? modelID, int quantity)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Products (CategoryID, ModelID, Price, ProductName, Description, Quantity)
                        VALUES (@CategoryID, @ModelID, @Price, @ProductName, @Description, @Quantity);
                        SELECT last_insert_rowid();
                    ";

                    command.Parameters.AddWithValue("@CategoryID", categoryID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ModelID", modelID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Price", price);
                    command.Parameters.AddWithValue("@ProductName", productName);
                    command.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Quantity", quantity);

                    object result = command.ExecuteScalar();
                    return result == null ? -1 : Convert.ToInt32((long)result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(AddNew), ex);
                throw new Exception("Error in AddNew Product: " + ex.Message);
            }
        }

        // ============================
        // UPDATE PRODUCT
        // ============================
        public static bool Update(
            int productID, string productName, string description, decimal price,
            int? categoryID, int? modelID, int quantity)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Products
                        SET ProductName  = @ProductName,
                            Description  = @Description,
                            Price        = @Price,
                            CategoryID   = @CategoryID,
                            ModelID      = @ModelID,
                            Quantity     = @Quantity
                        WHERE ProductID  = @ProductID;
                    ";

                    command.Parameters.AddWithValue("@ProductID", productID);
                    command.Parameters.AddWithValue("@ProductName", productName);
                    command.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Price", price);
                    command.Parameters.AddWithValue("@CategoryID", categoryID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ModelID", modelID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Quantity", quantity);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Update), ex);
                throw new Exception("Error in Update Product: " + ex.Message);
            }
        }

        // ============================
        // DELETE PRODUCT
        // ============================
        public static bool Delete(int productID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Products WHERE ProductID = @ProductID;";
                    command.Parameters.AddWithValue("@ProductID", productID);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Delete), ex);
                throw new Exception("Error in Delete Product: " + ex.Message);
            }
        }

        // ============================
        // GET ALL PRODUCTS
        // ============================
        public static DataTable GetAllProductDetails()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = new SqliteCommand("SELECT * FROM vw_ProductDetailsFull", connection))
                {
                    DataTable dt = new DataTable();
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    return dt;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetAllProductDetails), ex);
                throw new Exception("Error in GetAllProductDetails: " + ex.Message);
            }
        }

        // ============================
        // GET QUANTITY
        // ============================
        public static int GetQuantity(int productID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Quantity FROM Products WHERE ProductID = @ProductID;";
                    command.Parameters.AddWithValue("@ProductID", productID);

                    object result = command.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetQuantity), ex);
                throw new Exception("Error in GetQuantity: " + ex.Message);
            }
        }

        // ============================
        // GET PRODUCT ACTIVE STATUS
        // ============================
        public static bool GetActiveStatus(int productID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT IsActive
                        FROM Products
                        WHERE ProductID = @ProductID
                    ";

                    command.Parameters.AddWithValue("@ProductID", productID);

                    object result = command.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetActiveStatus), ex);
                throw new Exception("Error in GetActiveStatus Product: " + ex.Message);
            }
        }

        // ============================
        // ACTIVATE / DEACTIVATE PRODUCT
        // ============================
        public static bool SetActiveStatus(int productID, bool isActive)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Products
                        SET IsActive = @IsActive
                        WHERE ProductID = @ProductID
                    ";

                    command.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
                    command.Parameters.AddWithValue("@ProductID", productID);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(SetActiveStatus), ex);
                throw new Exception("Error in SetActiveStatus Product: " + ex.Message);
            }
        }

        // ============================
        // GET PRODUCT DEPENDENCIES
        // ============================
        public static int GetProductDependencies(int productID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT COUNT(*) FROM SalesDetails WHERE ProductID = @ProductID
            ";

                    command.Parameters.AddWithValue("@ProductID", productID);

                    object result = command.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetProductDependencies), ex);
                return 0;
            }
        }

    }
}