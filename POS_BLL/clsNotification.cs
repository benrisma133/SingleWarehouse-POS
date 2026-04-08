using System;
using System.Data;
using POS_DAL;

namespace POS_BLL
{
    public class clsNotification
    {
        public enum enType
        {
            OutOfStock = 1,
            LowStock = 2
        }

        public int NotificationID { get; set; }
        public int ProductID { get; set; }
        public enType Type { get; set; }
        public bool IsRead { get; set; }
        public bool IsSent { get; set; }
        public DateTime CreatedAt { get; set; }

        public clsNotification()
        {
            NotificationID = -1;
            ProductID = -1;
            Type = enType.LowStock;
            IsRead = false;
            IsSent = false;
            CreatedAt = DateTime.Now;
        }

        private clsNotification(int id, int productID, enType type, bool isRead, bool isSent, DateTime createdAt)
        {
            NotificationID = id;
            ProductID = productID;
            Type = type;
            IsRead = isRead;
            IsSent = isSent;
            CreatedAt = createdAt;
        }

        // ============================
        // CREATE (ADD IF NOT EXISTS)
        // ============================
        public bool Save()
        {
            this.NotificationID = clsNotificationsData.AddIfNotExists(this.ProductID, (int)this.Type);
            return this.NotificationID != -1;
        }

        // ============================
        // FIND BY ID
        // ============================
        public static clsNotification FindByID(int notificationID)
        {
            DataTable dt = clsNotificationsData.GetUnread(); // simple reuse

            foreach (DataRow row in dt.Rows)
            {
                if (Convert.ToInt32(row["NotificationID"]) == notificationID)
                {
                    return new clsNotification(
                        notificationID,
                        Convert.ToInt32(row["ProductID"]),
                        (enType)Convert.ToInt32(row["Type"]),
                        Convert.ToInt32(row["IsRead"]) == 1,
                        Convert.ToInt32(row["IsSent"]) == 1,
                        Convert.ToDateTime(row["CreatedAt"])
                    );
                }
            }

            return null;
        }

        // ============================
        // GET UNREAD
        // ============================
        public static DataTable GetUnread()
        {
            return clsNotificationsData.GetUnread();
        }

        // ============================
        // GET UNSENT
        // ============================
        public static DataTable GetUnsent()
        {
            return clsNotificationsData.GetUnsent();
        }

        // ============================
        // MARK AS READ
        // ============================
        public static bool MarkAsRead(int notificationID)
        {
            return clsNotificationsData.MarkAsRead(notificationID);
        }

        // ============================
        // MARK ALL AS READ
        // ============================
        public static void MarkAllAsRead()
        {
            clsNotificationsData.MarkAllAsRead();
        }

        // ============================
        // MARK AS SENT
        // ============================
        public static bool MarkAsSent(int notificationID)
        {
            return clsNotificationsData.MarkAsSent(notificationID);
        }

        // ============================
        // MARK ALL AS SENT
        // ============================
        public static void MarkAllAsSent()
        {
            clsNotificationsData.MarkAllAsSent();
        }

        // ============================
        // DELETE
        // ============================
        public static bool Delete(int notificationID)
        {
            return clsNotificationsData.Delete(notificationID);
        }

        // ============================
        // DELETE BY PRODUCT + TYPE
        // ============================
        public static void Delete(int productID, enType type)
        {
            clsNotificationsData.DeleteByProduct(productID, (int)type);
        }


        // ============================
        // GET ALL
        // ============================
        public static DataTable GetAll()
        {
            return clsNotificationsData.GetAll();
        }

    }
}