using POS_BLL;

namespace POS_BLL
{
    public static class clsNotify
    {
        public static void CheckStock(int productID, int quantity, int lowStockThreshold)
        {
            if (quantity == 0)
            {
                clsNotification.Delete(productID, clsNotification.enType.LowStock);

                new clsNotification
                {
                    ProductID = productID,
                    Type = clsNotification.enType.OutOfStock
                }.Save();
            }
            else if (quantity <= lowStockThreshold)
            {
                clsNotification.Delete(productID, clsNotification.enType.OutOfStock);

                new clsNotification
                {
                    ProductID = productID,
                    Type = clsNotification.enType.LowStock
                }.Save();
            }
            else
            {
                // stock is normal → remove notifications
                clsNotification.Delete(productID, clsNotification.enType.LowStock);
                clsNotification.Delete(productID, clsNotification.enType.OutOfStock);
            }
        }
    }
}