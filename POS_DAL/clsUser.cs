using Microsoft.Data.Sqlite;
using POS_DAL.Loggers;
using System;

namespace POS_DAL
{
    public static class clsUserData
    {
        private const string _className = nameof(clsUserData);

        // ============================
        // ADD USER (PERSON + USER)
        // ============================
        public static int Add(
            string firstName,
            string lastName,
            string dateOfBirth,
            string email,
            string phone,
            string address,
            int gender,
            string username,
            string passwordHash)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (var transaction = connection.BeginTransaction())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;

                    // 1. Insert Person
                    command.CommandText = @"
                        INSERT INTO Persons 
                        (FirstName, LastName, DateOfBirth, Email, Phone, Address, Gender)
                        VALUES (@FirstName, @LastName, @DOB, @Email, @Phone, @Address, @Gender);
                        SELECT last_insert_rowid();
                    ";

                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@LastName", lastName);
                    command.Parameters.AddWithValue("@DOB", dateOfBirth ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Email", email ?? "");
                    command.Parameters.AddWithValue("@Phone", phone ?? "");
                    command.Parameters.AddWithValue("@Address", address ?? "");
                    command.Parameters.AddWithValue("@Gender", gender);

                    object personIDResult = command.ExecuteScalar();
                    int personID = personIDResult == null ? -1 : Convert.ToInt32((long)personIDResult);

                    if (personID == -1)
                    {
                        transaction.Rollback();
                        return -1;
                    }

                    // 2. Insert User
                    command.Parameters.Clear();

                    command.CommandText = @"
                        INSERT INTO Users (PersonID, Username, PasswordHash)
                        VALUES (@PersonID, @Username, @Password);
                    ";

                    command.Parameters.AddWithValue("@PersonID", personID);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", passwordHash);

                    command.ExecuteNonQuery();

                    transaction.Commit();
                    return personID;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Add), ex);
                throw new Exception("Error in Add User: " + ex.Message);
            }
        }

        // ============================
        // UPDATE USER (PERSON + USER)
        // ============================
        public static bool Update(
            int personID,
            string firstName,
            string lastName,
            string dateOfBirth,
            string email,
            string phone,
            string address,
            int gender,
            string username,
            string passwordHash)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (var transaction = connection.BeginTransaction())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;

                    // 1. Update Person
                    command.CommandText = @"
                        UPDATE Persons SET
                            FirstName = @FirstName,
                            LastName = @LastName,
                            DateOfBirth = @DOB,
                            Email = @Email,
                            Phone = @Phone,
                            Address = @Address,
                            Gender = @Gender
                        WHERE PersonID = @PersonID
                    ";

                    command.Parameters.AddWithValue("@PersonID", personID);
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@LastName", lastName);
                    command.Parameters.AddWithValue("@DOB", dateOfBirth ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Email", email ?? "");
                    command.Parameters.AddWithValue("@Phone", phone ?? "");
                    command.Parameters.AddWithValue("@Address", address ?? "");
                    command.Parameters.AddWithValue("@Gender", gender);

                    command.ExecuteNonQuery();

                    // 2. Update User
                    command.Parameters.Clear();

                    command.CommandText = @"
                        UPDATE Users SET
                            Username = @Username,
                            PasswordHash = @Password
                        WHERE PersonID = @PersonID
                    ";

                    command.Parameters.AddWithValue("@PersonID", personID);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", passwordHash);

                    int rows = command.ExecuteNonQuery();

                    transaction.Commit();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Update), ex);
                return false;
            }
        }

        // ============================
        // LOGIN (RETURN PERSON ID)
        // ============================
        public static int GetPersonIDByUsernameAndPassword(string username, string passwordHash)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT PersonID
                        FROM Users
                        WHERE Username = @Username
                        AND PasswordHash = @Password
                        LIMIT 1
                    ";

                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", passwordHash);

                    object result = command.ExecuteScalar();

                    return result == null ? -1 : Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetPersonIDByUsernameAndPassword), ex);
                throw new Exception("Error in GetPersonIDByUsernameAndPassword: " + ex.Message);
            }
        }

        // ============================
        // CHECK USERNAME EXISTS
        // ============================
        public static bool IsUsernameExist(string username, int ignorePersonID = -1)
        {
            try
            {
                using (var connection = DbHelper.OpenConnection())
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 1
                        FROM Users
                        WHERE Username = @Username
                    ";

                    command.Parameters.AddWithValue("@Username", username);

                    if (ignorePersonID > 0)
                    {
                        command.CommandText += " AND PersonID <> @PersonID";
                        command.Parameters.AddWithValue("@PersonID", ignorePersonID);
                    }

                    command.CommandText += " LIMIT 1";

                    var result = command.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(IsUsernameExist), ex);
                throw new Exception("Error in IsUsernameExist: " + ex.Message);
            }
        }

        // ============================
        // CHANGE PASSWORD
        // ============================
        /// <summary>
        /// Verifies the old password hash first,
        /// then replaces it with the new one.
        /// Returns true on success, false if old
        /// password did not match or update failed.
        /// </summary>
        public static bool ChangePassword(
            int personID,
            string oldPasswordHash,
            string newPasswordHash)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (var transaction = connection.BeginTransaction())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;

                    // 1. Verify old password belongs to this user
                    command.CommandText = @"
                        SELECT COUNT(1)
                        FROM Users
                        WHERE PersonID = @PersonID
                        AND PasswordHash = @OldPassword
                    ";

                    command.Parameters.AddWithValue("@PersonID", personID);
                    command.Parameters.AddWithValue("@OldPassword", oldPasswordHash);

                    long matches = (long)command.ExecuteScalar();

                    if (matches == 0)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    // 2. Apply new password
                    command.Parameters.Clear();

                    command.CommandText = @"
                        UPDATE Users
                        SET PasswordHash = @NewPassword
                        WHERE PersonID = @PersonID
                    ";

                    command.Parameters.AddWithValue("@PersonID", personID);
                    command.Parameters.AddWithValue("@NewPassword", newPasswordHash);

                    int rows = command.ExecuteNonQuery();

                    if (rows == 0)
                    {
                        transaction.Rollback();
                        return false;
                    }

                    transaction.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(ChangePassword), ex);
                throw new Exception("Error in ChangePassword: " + ex.Message);
            }
        }

        // ============================
        // GET USER ACTIVE STATUS
        // ============================
        public static bool GetActiveStatus(int personID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT IsActive
                        FROM Users
                        WHERE PersonID = @PersonID
                    ";

                    command.Parameters.AddWithValue("@PersonID", personID);

                    object result = command.ExecuteScalar();
                    return result != null && Convert.ToInt32(result) == 1;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(GetActiveStatus), ex);
                throw new Exception("Error in GetActiveStatus User: " + ex.Message);
            }
        }

        // ============================
        // ACTIVATE / DEACTIVATE USER
        // ============================
        public static bool SetActiveStatus(int personID, bool isActive)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Users
                        SET IsActive = @IsActive
                        WHERE PersonID = @PersonID
                    ";

                    command.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
                    command.Parameters.AddWithValue("@PersonID", personID);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(SetActiveStatus), ex);
                throw new Exception("Error in SetActiveStatus User: " + ex.Message);
            }
        }
    }
}