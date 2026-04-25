using Microsoft.Data.Sqlite;
using POS_DAL.Loggers;
using System;
using System.Data;

namespace POS_DAL
{
    public static class clsModelData
    {
        private const string _className = nameof(clsModelData);

        // ============================
        // ADD NEW MODEL
        // ============================
        public static int AddNew(string name, string description, int? seriesID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Models (Name, Description, SeriesID)
                        VALUES (@Name, @Description, @SeriesID);
                        SELECT last_insert_rowid();
                    ";

                    command.Parameters.AddWithValue("@Name", name);

                    if (string.IsNullOrEmpty(description))
                        command.Parameters.AddWithValue("@Description", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@Description", description);

                    if (seriesID.HasValue)
                        command.Parameters.AddWithValue("@SeriesID", seriesID.Value);
                    else
                        command.Parameters.AddWithValue("@SeriesID", DBNull.Value);

                    object result = command.ExecuteScalar();
                    return result == null ? -1 : Convert.ToInt32((long)result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(AddNew), ex);
                throw new Exception("Error in AddNew Model: " + ex.Message);
            }
        }

        // ============================
        // UPDATE MODEL
        // ============================
        public static bool Update(int modelID, string name, string description, int? seriesID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Models
                        SET Name = @Name,
                            Description = @Description,
                            SeriesID = @SeriesID
                        WHERE ModelID = @ModelID;
                    ";

                    command.Parameters.AddWithValue("@ModelID", modelID);
                    command.Parameters.AddWithValue("@Name", name);

                    if (string.IsNullOrEmpty(description))
                        command.Parameters.AddWithValue("@Description", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@Description", description);

                    if (seriesID.HasValue)
                        command.Parameters.AddWithValue("@SeriesID", seriesID.Value);
                    else
                        command.Parameters.AddWithValue("@SeriesID", DBNull.Value);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Update), ex);
                throw new Exception("Error in Update Model: " + ex.Message);
            }
        }

        // ============================
        // DELETE MODEL
        // ============================
        public static bool Delete(int modelID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM Models
                        WHERE ModelID = @ModelID;
                    ";

                    command.Parameters.AddWithValue("@ModelID", modelID);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // FK violation — Model is linked to Products
                return false;
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Delete), ex);
                return false;
            }
        }

        // ============================
        // DELETE MODEL COMPLETELY
        // ============================
        public static bool DeleteModelCompletely(int modelID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (var transaction = connection.BeginTransaction())
                {
                    using (SqliteCommand cmd1 = connection.CreateCommand())
                    {
                        cmd1.CommandText = "DELETE FROM WarehouseModels WHERE ModelID = @ModelID;";
                        cmd1.Parameters.AddWithValue("@ModelID", modelID);
                        cmd1.ExecuteNonQuery();
                    }

                    using (SqliteCommand cmd2 = connection.CreateCommand())
                    {
                        cmd2.CommandText = "DELETE FROM Models WHERE ModelID = @ModelID;";
                        cmd2.Parameters.AddWithValue("@ModelID", modelID);
                        int rows = cmd2.ExecuteNonQuery();

                        transaction.Commit();
                        return rows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(DeleteModelCompletely), ex);
                throw new Exception("Error in DeleteModelCompletely: " + ex.Message);
            }
        }

        // ============================
        // GET MODEL BY ID
        // ============================
        public static bool GetByID(int modelID, ref string name, ref string description, ref int? seriesID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT Name, Description, SeriesID
                        FROM Models
                        WHERE ModelID = @ModelID;
                    ";
                    command.Parameters.AddWithValue("@ModelID", modelID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            name = reader["Name"].ToString();

                            description = reader["Description"] == DBNull.Value
                                ? null
                                : reader["Description"].ToString();

                            seriesID = reader["SeriesID"] == DBNull.Value
                                ? (int?)null
                                : Convert.ToInt32(reader["SeriesID"]);

                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetByID), ex);
                throw new Exception("Error in GetByID Model: " + ex.Message);
            }
        }

        // ============================
        // GET MODEL BY NAME
        // ============================
        public static bool GetByName(string name, ref int modelID, ref string description, ref int? seriesID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT ModelID, Description, SeriesID
                        FROM Models
                        WHERE Name = @Name
                        LIMIT 1;
                    ";
                    command.Parameters.AddWithValue("@Name", name);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            modelID = Convert.ToInt32(reader["ModelID"]);

                            description = reader["Description"] == DBNull.Value
                                ? null
                                : reader["Description"].ToString();

                            seriesID = reader["SeriesID"] == DBNull.Value
                                ? (int?)null
                                : Convert.ToInt32(reader["SeriesID"]);

                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetByName), ex);
                throw new Exception("Error in GetByName Model: " + ex.Message);
            }
        }

        // ============================
        // GET ALL MODELS
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
                    m.ModelID,
                    m.Name,
                    m.Description,
                    m.IsActive,
                    s.Name AS SeriesName,
                    s.SeriesID,
                    b.BrandID,
                    b.Name AS BrandName
                FROM Models m
                LEFT JOIN Series s ON m.SeriesID = s.SeriesID
                LEFT JOIN Brands b ON s.BrandID = b.BrandID;
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
                throw new Exception("Error in GetAll Models: " + ex.Message);
            }
        }

        // ============================
        // GET ALL DISTINCT MODELS
        // ============================
        public static DataTable GetAllDistinct()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT DISTINCT m.ModelID, m.Name, m.Description
                        FROM Models m
                        INNER JOIN WarehouseModels wm ON m.ModelID = wm.ModelID;
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
                clsLog.LogError(_className, nameof(GetAllDistinct), ex);
                throw new Exception("Error in GetAllDistinct Models: " + ex.Message);
            }
        }

        // ============================
        // CHECK IF MODEL EXISTS BY NAME
        // ============================
        public static bool IsModelExistByName(string name)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 1
                        FROM Models
                        WHERE Name = @Name COLLATE NOCASE
                        LIMIT 1;
                    ";

                    command.Parameters.AddWithValue("@Name", name);

                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(IsModelExistByName), ex);
                throw new Exception("Error in IsModelExistByName: " + ex.Message);
            }
        }

        // ============================
        // CHECK IF MODEL EXISTS BY NAME (IGNORE ID)
        // ============================
        public static bool IsModelExistByName(string name, int ignoreModelID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 1
                        FROM Models
                        WHERE Name = @Name COLLATE NOCASE
                          AND ModelID <> @ModelID
                        LIMIT 1;
                    ";

                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@ModelID", ignoreModelID);

                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(IsModelExistByName), ex);
                throw new Exception("Error in IsModelExistByName (ignoreModelID): " + ex.Message);
            }
        }

        // ============================
        // GET MODEL DEPENDENCIES
        // ============================
        public static int GetModelDependencies(int modelID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT COUNT(*) FROM Products WHERE ModelID = @ModelID
            ";

                    command.Parameters.AddWithValue("@ModelID", modelID);

                    object result = command.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetModelDependencies), ex);
                return 0;
            }
        }

        // ============================
        // GET MODEL ACTIVE STATUS
        // ============================
        public static bool GetActiveStatus(int modelID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT IsActive
                        FROM Models
                        WHERE ModelID = @ModelID
                    ";

                    command.Parameters.AddWithValue("@ModelID", modelID);

                    object result = command.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetActiveStatus), ex);
                throw new Exception("Error in GetActiveStatus Model: " + ex.Message);
            }
        }

        // ============================
        // ACTIVATE / DEACTIVATE MODEL
        // ============================
        public static bool SetActiveStatus(int modelID, bool isActive)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Models
                        SET IsActive = @IsActive
                        WHERE ModelID = @ModelID
                    ";

                    command.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
                    command.Parameters.AddWithValue("@ModelID", modelID);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(SetActiveStatus), ex);
                throw new Exception("Error in SetActiveStatus Model: " + ex.Message);
            }
        }
    }
}