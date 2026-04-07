using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_BLL
{
    public static class AppEvents
    {
        public static event Action StockChanged;

        public static void RaiseStockChanged()
        {
            StockChanged?.Invoke();
        }
    }
}
