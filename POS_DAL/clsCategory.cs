using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace POS_DAL
{
    public static class clsCategoryData
    {

        private static ILogger _logger;

        // Called ONCE from UI
        public static void InitLogger(ILoggerFactory factory)
        {
            _logger = factory.CreateLogger("DAL.clsCategoryData");
        }

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
                string msg = $"AddNew failed for Category '{name}': {ex.Message}";
                _logger?.LogError(ex, msg); // Logs exception + stack trace
                Debug.WriteLine(msg);        // Optional: also output in VS Output window
                return -1;
            }
        }


        // ============================
        // UPDATE CATEGORY
        // ============================
        public static bool Update(int categoryID, string name, string description, int? iconID = null)
        {
            using (var connection = DbHelper.OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE Categories
                    SET Name = @Name,
                        Description = @Description,
                        IconID = @IconID
                    WHERE CategoryID = @CategoryID;
                ";

                // ✅ Parameters
                command.Parameters.AddWithValue("@CategoryID", categoryID);
                command.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(name) ? (object)DBNull.Value : name);
                command.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                command.Parameters.AddWithValue("@IconID", iconID.HasValue ? (object)iconID.Value : DBNull.Value);


                int rows = command.ExecuteNonQuery();
                return rows > 0;
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
            catch (Exception ex)
            {
                throw new Exception("Error in Delete Category: " + ex.Message);
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
                throw new Exception("Error in GetAll Categories: " + ex.Message);
            }
        }

        // ============================
        // CHECK IF CATEGORY EXISTS BY NAME
        // ============================
        public static bool IsCategoryExistByName(string name)
        {
            using (var connection = DbHelper.OpenConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT 1
                    FROM Categories
                    WHERE Name = @Name COLLATE NOCASE
                    LIMIT 1;
                ";

                command.Parameters.AddWithValue("@Name", name.Trim());

                return command.ExecuteScalar() != null;
            }
        }

        public static bool IsCategoryExistByName(string name, int ignoreCategoryID)
        {
            using (var connection = DbHelper.OpenConnection())
            using (var command = connection.CreateCommand())
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

                return command.ExecuteScalar() != null;
            }
        }


    }
}
