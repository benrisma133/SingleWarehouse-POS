using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_BLL
{
    public class clsSaleDetails
    {
        // Get all sales (for DataGrid)
        public static DataTable GetAllSales(string filter = "")
        {
            return POS_DAL.clsSaleDetailsData.GetAllSales(filter);
        }

        // Get items of one sale
        public static DataTable GetSaleItems(int saleID)
        {
            if (saleID <= 0)
                return new DataTable();

            return POS_DAL.clsSaleDetailsData.GetSaleItems(saleID);
        }

        // Delete sale
        public static bool DeleteSale(int saleID)
        {
            if (saleID <= 0)
                return false;

            return POS_DAL.clsSaleDetailsData.DeleteSale(saleID);
        }
    }
}
