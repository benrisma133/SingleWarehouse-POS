using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS_DAL
{
    public static class clsDataAccessSettigs
    {

        

        public static string ConnectionString =
                 $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "POS.db")};";


        


    }
}
