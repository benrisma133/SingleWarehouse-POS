using Microsoft.Data.Sqlite;
using POS_DAL.Loggers;
using System;
using System.Data;

namespace POS_DAL
{
    public static class clsCategoryData
    {
        private const string _className = nameof(clsCategoryData);

        // ============================
        // ADD NEW CATEGORY
        // ============================
        public static int AddNew(string name, string description, int? iconID = null)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Categories (Name, Description, IconID)
                        VALUES (@Name, @Description, @IconID);
                        SELECT last_insert_rowid();
                    ";

                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue(
                        "@Description",
                        string.IsNullOrEmpty(description) ? (object)DBNull.Value : description
                    );
                    command.Parameters.AddWithValue("@IconID", iconID.HasValue ? (object)iconID.Value : DBNull.Value);

                    object result = command.ExecuteScalar();
                    return result == null ? -1 : Convert.ToInt32((long)result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(AddNew), ex);
                throw new Exception("Error in AddNew Category: " + ex.Message);
            }
        }

        // ============================
        // UPDATE CATEGORY
        // ============================
        public static bool Update(int categoryID, string name, string description, int? iconID = null)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Categories
                        SET Name = @Name,
                            Description = @Description,
                            IconID = @IconID
                        WHERE CategoryID = @CategoryID;
                    ";

                    command.Parameters.AddWithValue("@CategoryID", categoryID);
                    command.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(name) ? (object)DBNull.Value : name);
                    command.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                    command.Parameters.AddWithValue("@IconID", iconID.HasValue ? (object)iconID.Value : DBNull.Value);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Update), ex);
                throw new Exception("Error in Update Category: " + ex.Message);
            }
        }

        // ============================
        // DELETE CATEGORY
        // ============================
        public static bool Delete(int categoryID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM Categories
                        WHERE CategoryID = @CategoryID;
                    ";

                    command.Parameters.AddWithValue("@CategoryID", categoryID);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // FK violation — Category is linked to Products
                return false;
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Delete), ex);
                return false;
            }
        }

        // ============================
        // GET CATEGORY BY ID
        // ============================
        public static bool GetByID(int categoryID, ref string name, ref string description, ref int? iconID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT Name, Description, IconID
                        FROM Categories
                        WHERE CategoryID = @CategoryID;
                    ";

                    command.Parameters.AddWithValue("@CategoryID", categoryID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            name = reader["Name"].ToString();
                            description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString();
                            iconID = reader["IconID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["IconID"]);
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetByID), ex);
                throw new Exception("Error in GetByID Category: " + ex.Message);
            }
        }

        // ============================
        // GET CATEGORY BY NAME
        // ============================
        public static bool GetByName(string name, ref int categoryID, ref string description, ref int? iconID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT CategoryID, Description, IconID
                        FROM Categories
                        WHERE Name = @Name
                        LIMIT 1;
                    ";

                    command.Parameters.AddWithValue("@Name", name);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            categoryID = Convert.ToInt32(reader["CategoryID"]);
                            description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString();
                            iconID = reader["IconID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["IconID"]);
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetByName), ex);
                throw new Exception("Error in GetByName Category: " + ex.Message);
            }
        }

        // ============================
        // GET ALL CATEGORIES
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
                    c.CategoryID,
                    c.Name,
                    c.Description,
                    c.IsActive,
                    IFNULL(i.IconData, X'') AS IconData
                FROM Categories c
                LEFT JOIN CategoryIcons i ON c.IconID = i.IconID;
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
                throw new Exception("Error in GetAll Categories: " + ex.Message);
            }
        }

        // ============================
        // CHECK IF CATEGORY EXISTS BY NAME
        // ============================
        public static bool IsCategoryExistByName(string name)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 1
                        FROM Categories
                        WHERE Name = @Name COLLATE NOCASE
                        LIMIT 1;
                    ";

                    command.Parameters.AddWithValue("@Name", name.Trim());

                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(IsCategoryExistByName), ex);
                throw new Exception("Error in IsCategoryExistByName: " + ex.Message);
            }
        }

        // ============================
        // CHECK IF CATEGORY EXISTS BY NAME (IGNORE ID)
        // ============================
        public static bool IsCategoryExistByName(string name, int ignoreCategoryID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 1
                        FROM Categories
                        WHERE Name = @Name COLLATE NOCASE
                          AND CategoryID <> @CategoryID
                        LIMIT 1;
                    ";

                    command.Parameters.AddWithValue("@Name", name.Trim());
                    command.Parameters.AddWithValue("@CategoryID", ignoreCategoryID);

                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(IsCategoryExistByName), ex);
                throw new Exception("Error in IsCategoryExistByName (ignoreID): " + ex.Message);
            }
        }

        // ============================
        // GET CATEGORY DEPENDENCIES
        // ============================
        public static int GetCategoryDependencies(int categoryID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) FROM Products WHERE CategoryID = @CategoryID
                    ";

                    command.Parameters.AddWithValue("@CategoryID", categoryID);

                    object result = command.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetCategoryDependencies), ex);
                return 0;
            }
        }

        // ============================
        // GET CATEGORY ACTIVE STATUS
        // ============================
        public static bool GetActiveStatus(int categoryID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT IsActive
                FROM Categories
                WHERE CategoryID = @CategoryID
            ";

                    command.Parameters.AddWithValue("@CategoryID", categoryID);

                    object result = command.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetActiveStatus), ex);
                throw new Exception("Error in GetActiveStatus Category: " + ex.Message);
            }
        }

        // ============================
        // ACTIVATE / DEACTIVATE CATEGORY
        // ============================
        public static bool SetActiveStatus(int categoryID, bool isActive)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                UPDATE Categories
                SET IsActive = @IsActive
                WHERE CategoryID = @CategoryID
            ";

                    command.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
                    command.Parameters.AddWithValue("@CategoryID", categoryID);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(SetActiveStatus), ex);
                throw new Exception("Error in SetActiveStatus Category: " + ex.Message);
            }
        }


    }
}