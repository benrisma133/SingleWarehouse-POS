using Microsoft.Data.Sqlite;
using POS_DAL.Loggers;
using System;
using System.Data;

namespace POS_DAL
{
    public static class clsSeriesData
    {
        private const string _className = nameof(clsSeriesData);

        // ============================
        // ADD NEW SERIES
        // ============================
        public static int AddNew(int brandID, string name, string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Series (BrandID, Name, Description)
                        VALUES (@BrandID, @Name, @Description);
                        SELECT last_insert_rowid();
                    ";
                    command.Parameters.AddWithValue("@BrandID", brandID);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Description", description ?? "");

                    object result = command.ExecuteScalar();
                    return result == null ? -1 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(AddNew), ex);
                throw new Exception("Error in AddNew Series: " + ex.Message);
            }
        }

        // ============================
        // UPDATE SERIES
        // ============================
        public static bool Update(int seriesID, int brandID, string name, string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Series
                        SET BrandID = @BrandID,
                            Name = @Name,
                            Description = @Description
                        WHERE SeriesID = @SeriesID
                    ";
                    command.Parameters.AddWithValue("@SeriesID", seriesID);
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
                throw new Exception("Error in Update Series: " + ex.Message);
            }
        }

        // ============================
        // DELETE SERIES
        // ============================
        public static bool Delete(int seriesID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM Series
                        WHERE SeriesID = @SeriesID
                    ";
                    command.Parameters.AddWithValue("@SeriesID", seriesID);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // FK violation — Series is linked to Models
                return false;
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Delete), ex);
                return false;
            }
        }

        // ============================
        // GET SERIES BY ID
        // ============================
        public static bool GetByID(int seriesID, ref int brandID, ref string name, ref string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT BrandID, Name, Description
                        FROM Series
                        WHERE SeriesID = @SeriesID
                    ";
                    command.Parameters.AddWithValue("@SeriesID", seriesID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            brandID = Convert.ToInt32(reader["BrandID"]);
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
                throw new Exception("Error in GetByID Series: " + ex.Message);
            }
        }

        // ============================
        // GET SERIES BY NAME
        // ============================
        public static bool GetByName(string name, ref int seriesID, ref int brandID, ref string description)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT SeriesID, BrandID, Description
                        FROM Series
                        WHERE Name = @Name
                        LIMIT 1
                    ";
                    command.Parameters.AddWithValue("@Name", name);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            seriesID = Convert.ToInt32(reader["SeriesID"]);
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
                throw new Exception("Error in GetByName Series: " + ex.Message);
            }
        }

        // ============================
        // GET ALL SERIES
        // ============================
        public static DataTable GetAll()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            s.SeriesID,
                            s.BrandID,
                            b.Name AS BrandName,
                            s.Name,
                            s.IsActive,
                            s.Description
                        FROM Series s
                        INNER JOIN Brands b ON s.BrandID = b.BrandID
                        ORDER BY s.SeriesID
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
                throw new Exception("Error in GetAll Series: " + ex.Message);
            }
        }

        // ============================
        // GET SERIES BY BRAND ID
        // ============================
        public static DataTable GetByBrandID(int brandID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            s.SeriesID,
                            s.BrandID,
                            b.Name AS BrandName,
                            s.Name,
                            s.Description
                        FROM Series s
                        INNER JOIN Brands b ON s.BrandID = b.BrandID
                        WHERE s.BrandID = @BrandID
                        ORDER BY s.SeriesID
                    ";

                    command.Parameters.AddWithValue("@BrandID", brandID);

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
                clsLog.LogError(_className, nameof(GetByBrandID), ex);
                throw new Exception("Error in GetByBrandID Series: " + ex.Message);
            }
        }

        // ============================
        // CHECK IF SERIES EXISTS BY NAME
        // ============================
        public static bool IsSeriesExistByName(string name, int ignoreSeriesID = -1)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 1
                        FROM Series
                        WHERE Name = @Name COLLATE NOCASE
                    ";

                    command.Parameters.AddWithValue("@Name", name);

                    if (ignoreSeriesID > 0)
                    {
                        command.CommandText += " AND SeriesID <> @SeriesID";
                        command.Parameters.AddWithValue("@SeriesID", ignoreSeriesID);
                    }

                    command.CommandText += " LIMIT 1";

                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(IsSeriesExistByName), ex);
                throw new Exception("Error in IsSeriesExistByName: " + ex.Message);
            }
        }

        // ============================
        // GET SERIES DEPENDENCIES
        // ============================
        public static (int ModelsCount, int ProductsCount) GetSeriesDependencies(int seriesID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT
                            (SELECT COUNT(*) FROM Models WHERE SeriesID = @SeriesID) AS ModelsCount,

                            (SELECT COUNT(*)
                             FROM Products p
                             INNER JOIN Models m ON p.ModelID = m.ModelID
                             WHERE m.SeriesID = @SeriesID) AS ProductsCount
                    ";

                    command.Parameters.AddWithValue("@SeriesID", seriesID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int models = Convert.ToInt32(reader["ModelsCount"]);
                            int products = Convert.ToInt32(reader["ProductsCount"]);
                            return (models, products);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetSeriesDependencies), ex);
            }

            return (0, 0);
        }

        // ============================
        // GET SERIES ACTIVE STATUS
        // ============================
        public static bool GetActiveStatus(int seriesID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT IsActive
                        FROM Series
                        WHERE SeriesID = @SeriesID
                    ";

                    command.Parameters.AddWithValue("@SeriesID", seriesID);

                    object result = command.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetActiveStatus), ex);
                throw new Exception("Error in GetActiveStatus Series: " + ex.Message);
            }
        }

        // ============================
        // ACTIVATE / DEACTIVATE SERIES
        // ============================
        public static bool SetActiveStatus(int seriesID, bool isActive)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Series
                        SET IsActive = @IsActive
                        WHERE SeriesID = @SeriesID
                    ";

                    command.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
                    command.Parameters.AddWithValue("@SeriesID", seriesID);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(SetActiveStatus), ex);
                throw new Exception("Error in SetActiveStatus Series: " + ex.Message);
            }
        }
    }
}