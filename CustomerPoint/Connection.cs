using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerPoint
{
    public class Connection
    {
        public static SqlConnection con = new SqlConnection("Data Source=DESKTOP-4F2UI6D;Initial Catalog=BankApp;Integrated Security=True");
        public static string salt = "EDITH";

        public static List<Transactions> transactions = new List<Transactions>();
    }
}
