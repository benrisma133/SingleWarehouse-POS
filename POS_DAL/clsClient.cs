using Microsoft.Data.Sqlite;
using POS_DAL.Loggers;
using System;
using System.Data;

namespace POS_DAL
{
    public static class clsClientData
    {
        private const string _className = nameof(clsClientData);

        // ============================
        // ADD NEW CLIENT
        // ============================
        public static int AddNew(string firstName, string lastName, string phone, string email)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Clients (FirstName, LastName, Phone, Email)
                        VALUES (@FirstName, @LastName, @Phone, @Email);
                        SELECT last_insert_rowid();
                    ";

                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@LastName", lastName);

                    if (string.IsNullOrEmpty(phone))
                        command.Parameters.AddWithValue("@Phone", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@Phone", phone);

                    if (string.IsNullOrEmpty(email))
                        command.Parameters.AddWithValue("@Email", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@Email", email);

                    object result = command.ExecuteScalar();
                    return result == null ? -1 : Convert.ToInt32((long)result);
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // Unique constraint violation for email/phone
                clsLog.LogError(_className, nameof(AddNew), ex);
                return -1;
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(AddNew), ex);
                throw new Exception("Error in AddNew Client: " + ex.Message);
            }
        }

        // ============================
        // UPDATE CLIENT BY ID
        // ============================
        public static bool Update(int clientID, string firstName, string lastName, string phone, string email)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Clients
                        SET FirstName = @FirstName,
                            LastName = @LastName,
                            Phone = @Phone,
                            Email = @Email
                        WHERE ClientID = @ClientID
                    ";

                    command.Parameters.AddWithValue("@ClientID", clientID);
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@LastName", lastName);

                    if (string.IsNullOrEmpty(phone))
                        command.Parameters.AddWithValue("@Phone", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@Phone", phone);

                    if (string.IsNullOrEmpty(email))
                        command.Parameters.AddWithValue("@Email", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@Email", email);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // Unique constraint violation
                clsLog.LogError(_className, nameof(Update), ex);
                return false;
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Update), ex);
                throw new Exception("Error in Update Client: " + ex.Message);
            }
        }

        // ============================
        // DELETE CLIENT BY ID
        // ============================
        public static bool Delete(int clientID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM Clients
                        WHERE ClientID = @ClientID
                    ";
                    command.Parameters.AddWithValue("@ClientID", clientID);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                // FK violation — Client has sales
                return false;
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Delete), ex);
                return false;
            }
        }

        // ============================
        // GET CLIENT BY ID
        // ============================
        public static bool GetByID(int clientID, ref string firstName, ref string lastName, ref string phone, ref string email)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT FirstName, LastName, Phone, Email
                        FROM Clients
                        WHERE ClientID = @ClientID
                    ";
                    command.Parameters.AddWithValue("@ClientID", clientID);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            firstName = reader["FirstName"].ToString();
                            lastName = reader["LastName"].ToString();

                            if (reader["Phone"] != DBNull.Value)
                                phone = reader["Phone"].ToString();
                            else
                                phone = "";

                            if (reader["Email"] != DBNull.Value)
                                email = reader["Email"].ToString();
                            else
                                email = "";

                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetByID), ex);
                throw new Exception("Error in GetByID Client: " + ex.Message);
            }
        }

        // ============================
        // GET ALL CLIENTS
        // ============================
        public static DataTable GetAll()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT ClientID, FirstName, LastName, Phone, Email, IsActive
                FROM Clients
                ORDER BY ClientID
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
                throw new Exception("Error in GetAll Clients: " + ex.Message);
            }
        }

        // ============================
        // CHECK IF EMAIL EXISTS (OPTIONAL IGNORE CLIENTID)
        // ============================
        public static bool IsEmailExist(string email, int ignoreClientID = -1)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 1
                        FROM Clients
                        WHERE Email = @Email
                    ";

                    if (string.IsNullOrEmpty(email))
                        command.Parameters.AddWithValue("@Email", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@Email", email);

                    if (ignoreClientID > 0)
                    {
                        command.CommandText += " AND ClientID <> @ClientID";
                        command.Parameters.AddWithValue("@ClientID", ignoreClientID);
                    }

                    command.CommandText += " LIMIT 1";

                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(IsEmailExist), ex);
                throw new Exception("Error in IsEmailExist Client: " + ex.Message);
            }
        }

        // ============================
        // CHECK IF PHONE EXISTS (OPTIONAL IGNORE CLIENTID)
        // ============================
        public static bool IsPhoneExist(string phone, int ignoreClientID = -1)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 1
                        FROM Clients
                        WHERE Phone = @Phone
                    ";

                    if (string.IsNullOrEmpty(phone))
                        command.Parameters.AddWithValue("@Phone", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@Phone", phone);

                    if (ignoreClientID > 0)
                    {
                        command.CommandText += " AND ClientID <> @ClientID";
                        command.Parameters.AddWithValue("@ClientID", ignoreClientID);
                    }

                    command.CommandText += " LIMIT 1";

                    object result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(IsPhoneExist), ex);
                throw new Exception("Error in IsPhoneExist Client: " + ex.Message);
            }
        }

        // ============================
        // GET CLIENT DEPENDENCIES
        // ============================
        public static int GetClientDependencies(int clientID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) FROM Sales WHERE ClientID = @ClientID
                    ";

                    command.Parameters.AddWithValue("@ClientID", clientID);

                    object result = command.ExecuteScalar();
                    return result == null ? 0 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetClientDependencies), ex);
                return 0;
            }
        }

        // ============================
        // GET CLIENT ACTIVE STATUS
        // ============================
        public static bool GetActiveStatus(int clientID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT IsActive
                        FROM Clients
                        WHERE ClientID = @ClientID
                    ";

                    command.Parameters.AddWithValue("@ClientID", clientID);

                    object result = command.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetActiveStatus), ex);
                throw new Exception("Error in GetActiveStatus Client: " + ex.Message);
            }
        }

        // ============================
        // ACTIVATE / DEACTIVATE CLIENT
        // ============================
        public static bool SetActiveStatus(int clientID, bool isActive)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Clients
                        SET IsActive = @IsActive
                        WHERE ClientID = @ClientID
                    ";

                    command.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
                    command.Parameters.AddWithValue("@ClientID", clientID);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(SetActiveStatus), ex);
                throw new Exception("Error in SetActiveStatus Client: " + ex.Message);
            }
        }


    }
}