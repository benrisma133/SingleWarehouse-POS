using Microsoft.Data.Sqlite;
using POS_DAL.Loggers;
using System;
using System.Data;

namespace POS_DAL
{
    public static class clsBrandsData
    {
        private const string _className = nameof(clsBrandsData);

        // ============================
        // ADD NEW BRAND
        // ============================
        public static int AddNew(string name, string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Brands (Name, Description)
                        VALUES (@Name, @Description);
                        SELECT last_insert_rowid();
                    ";
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Description", description ?? "");

                    object result = command.ExecuteScalar();
                    return result == null ? -1 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(AddNew), ex);
                throw new Exception("Error in AddNew Brand: " + ex.Message);
            }
        }

        // ============================
        // UPDATE BRAND
        // ============================
        public static bool Update(int brandID, string name, string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Brands
                        SET Name = @Name,
                            Description = @Description
                        WHERE BrandID = @BrandID
                    ";
                    command.Parameters.AddWithValue("@BrandID", brandID);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Description", description ?? "");

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Update), ex);
                throw new Exception("Error in Update Brand: " + ex.Message);
            }
        }

        // ============================
        // DELETE BRAND
        // ============================
        public static bool Delete(int brandID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM Brands
                        WHERE BrandID = @BrandID
                    ";
                    command.Parameters.AddWithValue("@BrandID", brandID);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // FK violation — expected business rule, not a real error
                return false;
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Delete), ex);
                return false;
            }
        }

        // ============================
        // GET BRAND BY ID
        // ============================
        public static bool GetByID(int brandID, ref string name, ref string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT Name, Description
                        FROM Brands
                        WHERE BrandID = @BrandID
                    ";
                    command.Parameters.AddWithValue("@BrandID", brandID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            name = reader["Name"].ToString();
                            description = reader["Description"].ToString();
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetByID), ex);
                throw new Exception("Error in GetByID Brand: " + ex.Message);
            }
        }

        // ============================
        // GET BRAND BY NAME
        // ============================
        public static bool GetByName(string name, ref int brandID, ref string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT BrandID, Description
                        FROM Brands
                        WHERE Name = @Name
                        LIMIT 1
                    ";
                    command.Parameters.AddWithValue("@Name", name);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            brandID = Convert.ToInt32(reader["BrandID"]);
                            description = reader["Description"].ToString();
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetByName), ex);
                throw new Exception("Error in GetByName Brand: " + ex.Message);
            }
        }

        // ============================
        // GET ALL BRANDS
        // ============================
        public static DataTable GetAll()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT BrandID, Name, Description, IsActive
                FROM Brands
                ORDER BY BrandID
            ";

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetAll), ex);
                throw new Exception("Error in GetAll Brands: " + ex.Message);
            }
        }

        // ============================
        // CHECK IF BRAND EXISTS BY NAME
        // ============================
        public static bool IsBrandExistByName(string name, int ignoreBrandID = -1)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 1
                        FROM Brands
                        WHERE Name = @Name COLLATE NOCASE
                    ";
                    command.Parameters.AddWithValue("@Name", name);

                    if (ignoreBrandID > 0)
                    {
                        command.CommandText += " AND BrandID <> @BrandID";
                        command.Parameters.AddWithValue("@BrandID", ignoreBrandID);
                    }

                    command.CommandText += " LIMIT 1";

                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(IsBrandExistByName), ex);
                throw new Exception("Error in IsBrandExistByName: " + ex.Message);
            }
        }


        // ============================
        // GET BRAND DEPENDENCIES
        // ============================
        public static (int SeriesCount, int ModelsCount, int ProductsCount) GetBrandDependencies(int brandID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT
                    (SELECT COUNT(*) FROM Series WHERE BrandID = @BrandID) AS SeriesCount,

                    (SELECT COUNT(*)
                     FROM Models m
                     INNER JOIN Series s ON m.SeriesID = s.SeriesID
                     WHERE s.BrandID = @BrandID) AS ModelsCount,

                    (SELECT COUNT(*)
                     FROM Products p
                     INNER JOIN Models m ON p.ModelID = m.ModelID
                     INNER JOIN Series s ON m.SeriesID = s.SeriesID
                     WHERE s.BrandID = @BrandID) AS ProductsCount
            ";

                    command.Parameters.AddWithValue("@BrandID", brandID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int series = Convert.ToInt32(reader["SeriesCount"]);
                            int models = Convert.ToInt32(reader["ModelsCount"]);
                            int products = Convert.ToInt32(reader["ProductsCount"]);

                            return (series, models, products);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetBrandDependencies), ex);
            }

            return (0, 0, 0);
        }


        // ============================
        // GET BRAND ACTIVE STATUS
        // ============================
        public static bool GetActiveStatus(int brandId)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT IsActive
                FROM Brands
                WHERE BrandID = @BrandID
            ";

                    command.Parameters.AddWithValue("@BrandID", brandId);

                    object result = command.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetActiveStatus), ex);
                throw new Exception("Error in GetActiveStatus Brand: " + ex.Message);
            }
        }

        // ============================
        // ACTIVATE / DEACTIVATE BRAND
        // ============================
        public static bool SetActiveStatus(int brandId, bool isActive)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                UPDATE Brands
                SET IsActive = @IsActive
                WHERE BrandID = @BrandID
            ";

                    command.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
                    command.Parameters.AddWithValue("@BrandID", brandId);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(SetActiveStatus), ex);
                throw new Exception("Error in SetActiveStatus Brand: " + ex.Message);
            }
        }


    }
}