using Microsoft.Data.Sqlite;
using POS_DAL.Loggers;
using System;
using System.Data;

namespace POS_DAL
{
    public static class clsNotificationsData
    {
        private const string _className = nameof(clsNotificationsData);

        // ============================
        // ADD NOTIFICATION (NO DUPLICATES)
        // ============================
        public static int AddIfNotExists(int productID, int type)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    // check first
                    command.CommandText = @"
                        SELECT NotificationID 
                        FROM Notifications 
                        WHERE ProductID = @ProductID AND Type = @Type
                        LIMIT 1
                    ";
                    command.Parameters.AddWithValue("@ProductID", productID);
                    command.Parameters.AddWithValue("@Type", type);

                    object existing = command.ExecuteScalar();

                    if (existing != null)
                        return Convert.ToInt32(existing);

                    // insert if not exists
                    command.Parameters.Clear();
                    command.CommandText = @"
                        INSERT INTO Notifications (ProductID, Type)
                        VALUES (@ProductID, @Type);
                        SELECT last_insert_rowid();
                    ";
                    command.Parameters.AddWithValue("@ProductID", productID);
                    command.Parameters.AddWithValue("@Type", type);

                    object result = command.ExecuteScalar();
                    return result == null ? -1 : Convert.ToInt32((long)result);
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(AddIfNotExists), ex);
                throw new Exception("Error in AddIfNotExists Notification: " + ex.Message);
            }
        }

        // ============================
        // MARK AS READ
        // ============================
        public static bool MarkAsRead(int notificationID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Notifications
                        SET IsRead = 1
                        WHERE NotificationID = @ID
                    ";
                    command.Parameters.AddWithValue("@ID", notificationID);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(MarkAsRead), ex);
                throw new Exception("Error in MarkAsRead: " + ex.Message);
            }
        }

        // ============================
        // MARK ALL AS READ
        // ============================
        public static bool MarkAllAsRead()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Notifications SET IsRead = 1";
                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(MarkAllAsRead), ex);
                throw new Exception("Error in MarkAllAsRead: " + ex.Message);
            }
        }

        // ============================
        // MARK AS SENT
        // ============================
        public static bool MarkAsSent(int notificationID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Notifications
                        SET IsSent = 1
                        WHERE NotificationID = @ID
                    ";
                    command.Parameters.AddWithValue("@ID", notificationID);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(MarkAsSent), ex);
                throw new Exception("Error in MarkAsSent: " + ex.Message);
            }
        }

        // ============================
        // GET UNREAD NOTIFICATIONS
        // ============================
        public static DataTable GetUnread()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT *
                        FROM Notifications
                        WHERE IsRead = 0
                        ORDER BY CreatedAt DESC
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
                clsLog.LogError(_className, nameof(GetUnread), ex);
                throw new Exception("Error in GetUnread: " + ex.Message);
            }
        }

        // ============================
        // GET UNSENT NOTIFICATIONS
        // ============================
        public static DataTable GetUnsent()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT *
                        FROM Notifications
                        WHERE IsSent = 0
                        ORDER BY CreatedAt DESC
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
                clsLog.LogError(_className, nameof(GetUnsent), ex);
                throw new Exception("Error in GetUnsent: " + ex.Message);
            }
        }

        // ============================
        // DELETE NOTIFICATION
        // ============================
        public static bool Delete(int notificationID)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM Notifications
                        WHERE NotificationID = @ID
                    ";
                    command.Parameters.AddWithValue("@ID", notificationID);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(Delete), ex);
                throw new Exception("Error in Delete Notification: " + ex.Message);
            }
        }

        // ============================
        // DELETE BY PRODUCT + TYPE (optional cleanup)
        // ============================
        public static bool DeleteByProduct(int productID, int type)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        DELETE FROM Notifications
                        WHERE ProductID = @ProductID AND Type = @Type
                    ";
                    command.Parameters.AddWithValue("@ProductID", productID);
                    command.Parameters.AddWithValue("@Type", type);

                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(DeleteByProduct), ex);
                throw new Exception("Error in DeleteByProduct: " + ex.Message);
            }
        }

        // ============================
        // MARK ALL AS SENT
        // ============================
        public static bool MarkAllAsSent()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Notifications SET IsSent = 1 WHERE IsSent = 0";
                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(MarkAllAsSent), ex);
                throw new Exception("Error in MarkAllAsSent: " + ex.Message);
            }
        }

        // ============================
        // MARK AS READ BY PRODUCT + TYPE
        // ============================
        public static bool MarkAsReadByProduct(int productID, int type)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Notifications
                        SET IsRead = 1
                        WHERE ProductID = @ProductID AND Type = @Type
                    ";
                    command.Parameters.AddWithValue("@ProductID", productID);
                    command.Parameters.AddWithValue("@Type", type);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(MarkAsReadByProduct), ex);
                throw new Exception("Error in MarkAsReadByProduct: " + ex.Message);
            }
        }

        // ============================
        // MARK AS SENT BY PRODUCT + TYPE
        // ============================
        public static bool MarkAsSentByProduct(int productID, int type)
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Notifications
                        SET IsSent = 1
                        WHERE ProductID = @ProductID AND Type = @Type
                    ";
                    command.Parameters.AddWithValue("@ProductID", productID);
                    command.Parameters.AddWithValue("@Type", type);

                    return command.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                clsLog.LogError(_className, nameof(MarkAsSentByProduct), ex);
                throw new Exception("Error in MarkAsSentByProduct: " + ex.Message);
            }
        }

        // ============================
        // GET ALL NOTIFICATIONS
        // ============================
        public static DataTable GetAll()
        {
            try
            {
                using (SqliteConnection connection = DbHelper.OpenConnection())
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT *
                        FROM Notifications
                        ORDER BY CreatedAt DESC
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
                throw new Exception("Error in GetAll Notifications: " + ex.Message);
            }
        }
    }
}