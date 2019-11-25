using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace CustomerPoint
{
    public class CustomerFunctions
    {
        static SqlConnection con = Connection.con;
        public static string CustomerLogin (CLoginInfo info)
        {
            
            string username = info.Username;
            string password = info.Password;
            string encrypt = SHA1(password + Connection.salt);
            string login = " Invalid Login Details";

            string query = "SELECT * FROM Customers WHERE CustomerUsername = @username AND CustomerPassword = @hash";
            SqlCommand command = new SqlCommand(query, con);

            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@hash", encrypt);

            try
            {
                con.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    login = "Login Successful";
                }
                else
                {
                    login = "Invalid Not Found";
                }
                con.Close();
            }
            catch (Exception ex)
            {
                login = "Error: " + ex.Message;
                con.Close();
            }

            return login;
        }

        public static string SHA1(string input)
        {
            byte[] hash;

            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                hash = sha1.ComputeHash(Encoding.Unicode.GetBytes(input));
            }

            var sb = new StringBuilder();

            foreach (byte b in hash) sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }

        public static double Balance(CAccountNumber number)
        {
            string acctNo = number.AcctNo;

            double amnt = BalCheck(acctNo);
       
            return amnt;
        }

        public static void History(CAccountNumber number)
        {
            Connection.transactions.Clear();
            string acctNo = number.AcctNo;

            string query = "SELECT TransactionDescription, TransactionAmount, TransactionTime FROM History WHERE TransactionAccount = @acct";
            SqlCommand command = new SqlCommand(query, con);
            command.Parameters.AddWithValue("@acct", acctNo);

            try
            {
                con.Open();
                SqlDataReader reader = command.ExecuteReader();

                while(reader.Read())
                {
                    Transactions transactions = new Transactions();

                    transactions.Description = reader.GetString(0);
                    transactions.Amount = reader.GetDouble(1);
                    transactions.time = reader.GetDateTime(2);

                    Connection.transactions.Add(transactions);
                }
                
                con.Close();
                
            }
            catch (Exception )
            {
                con.Close();
            }
        }

        public static double BalCheck(string number)
        {
            string acctNo = number;
            double amnt = 0.0;

            string query = "SELECT AccountBalance FROM Accounts WHERE AccountNumber = @acct";
            SqlCommand command = new SqlCommand(query, con);

            command.Parameters.AddWithValue("@acct", acctNo);

            try
            {
                con.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    amnt = reader.GetDouble(0);
                }
                else
                {
                    amnt = 1;
                }
                con.Close();
            }
            catch (Exception)
            {

                con.Close();
            }

            return amnt;
        } 

        public static string AcctType(string accountNumber)
        {
            string acctNo = accountNumber;
            string type = "UnKnown";
            string query = "SELECT AccountType FROM Accounts WHERE AccountNumber = @acct";
            SqlCommand command = new SqlCommand(query, con);

            command.Parameters.AddWithValue("@acct", acctNo);

            try
            {
                con.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    type = reader.GetString(0);
                }
                else
                {
                    type = "Invalid Account Number";
                }
                con.Close();
            }
            catch (Exception)
            {

                con.Close();
            }

            return type;
        }

        public static int ID(string number)
        {
            string acctNo = number;
            int id = 0;

            string query = "SELECT CustomerID FROM Accounts WHERE AccountNumber = @acct";
            SqlCommand command = new SqlCommand(query, con);

            command.Parameters.AddWithValue("@acct", acctNo);
            try
            {
                con.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = reader.GetInt32(0);
                }
                else
                {
                    id = 0;
                }
                con.Close();
            }
            catch (Exception)
            {

                con.Close();
            }
            return id;
        }

        public static string Names(string number)
        {
            string acctNo = number;
            int id = ID(number);
            string names = "Unknown";

            string query = "SELECT CustomerFirstName, CustomerLastName FROM Customers WHERE CustomerID = @id";
            SqlCommand command = new SqlCommand(query, con);
            command.Parameters.AddWithValue("@id", id);

            try
            {
                con.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    names = reader.GetString(0) + " " + reader.GetString(1);
                }
                else names = "Invalid Account Number";
                con.Close();
            }
            catch (Exception)
            {
                con.Close();
            }

            return names;
        }

        public static string Withdrawal(CAccountNumber cAccount)
        {
            string acctNo = cAccount.AcctNo;
            double balance = BalCheck(acctNo);
            string type = AcctType(acctNo);
            double amount = cAccount.Amount;
            string message = "";

            if ((balance - amount >= 1000 && type == "Savings") || (balance - amount >= 0 && type == "Current"))
            {
                balance -= amount;
                
                int id = ID(acctNo);

                string query = "UPDATE Accounts SET AccountBalance = @amnt WHERE AccountNumber = @acctNo";
                string secQuery = "INSERT INTO History VALUES(@desc, @amnt, @acctNo, @created, @id)";

                SqlCommand com = new SqlCommand(query, con);
                SqlCommand mand = new SqlCommand(secQuery, con);

                com.Parameters.AddWithValue("@acctNo", acctNo);
                com.Parameters.AddWithValue("@amnt", balance);

                mand.Parameters.AddWithValue("@desc", WithdrawMessage());
                mand.Parameters.AddWithValue("@amnt", -amount);
                mand.Parameters.AddWithValue("@acctNo", acctNo);
                mand.Parameters.AddWithValue("@created", DateTime.Now);
                mand.Parameters.AddWithValue("@id", id);

                con.Open();

                int done = com.ExecuteNonQuery();
                int made = mand.ExecuteNonQuery();


                if (done >= 0 && made >= 0)
                {
                    message = "Withdrawal Successful";
                    con.Close();
                }
                else con.Close();
            }
            else
            {
                message = "Insufficient Balance";
            }
            return message;
        }

        public static string DepositMessage()
        {
            string[] dep = new string[]
            {
                "Bank Deposit @ Ajah Branch",
                "ATM Deposit @ Surulere",
                "Bank Deposit @ Igbo Efon"
            };

            Random rand = new Random();
            int random = rand.Next(3);

            return dep[random];
        }

        public static string WithdrawMessage()
        {
            string[] dep = new string[]
            {
                "Bank Withdrawal @ Ajah Branch",
                "ATM Withdrawal @ Surulere",
                "Cheque cashed @ Igbo Efon Branch",
                "JumiaPay Services @WEBGATEWAY",
                "POS 12908 @ Genesis Cinema"
            };

            Random rand = new Random();
            int random = rand.Next(5);

            return dep[random];
        }

        public static string Transfer(CAccountNumber number)
        {
            string acctNo = number.AcctNo;
            string tempAcctNo = number.TempAcctNo;
            double balance = BalCheck(acctNo);
            double tempBal = BalCheck(tempAcctNo);
            string type = AcctType(acctNo);
            double amount = number.Amount;
            string message = "";

            if ((balance - amount >= 1000 && type == "Savings") || (balance - amount >= 0 && type == "Current"))
            {
                balance -= amount;
                tempBal += amount;

                int outID = ID(acctNo);
                int inID = ID(tempAcctNo);

                string outName = Names(acctNo);
                string inName = Names(tempAcctNo);

                string query = "UPDATE Accounts SET AccountBalance = @amnt WHERE AccountNumber = @acctNo";
                string secQuery = "INSERT INTO History VALUES(@desc, @amnt, @acctNo, @created, @id)";

                string tOut = "Transfer sent to " + inName + " " + tempAcctNo;
                string tIn = "TF received FM " + outName + " " + acctNo;

                SqlCommand com = new SqlCommand(query, con);
                SqlCommand mand = new SqlCommand(secQuery, con);

                com.Parameters.AddWithValue("@acctNo", acctNo);
                com.Parameters.AddWithValue("@amnt", balance);

                mand.Parameters.AddWithValue("@desc", tOut);
                mand.Parameters.AddWithValue("@amnt", -amount);
                mand.Parameters.AddWithValue("@acctNo", acctNo);
                mand.Parameters.AddWithValue("@created", DateTime.Now);
                mand.Parameters.AddWithValue("@id", outID);

                SqlCommand com1 = new SqlCommand(query, con);
                SqlCommand mand1 = new SqlCommand(secQuery, con);

                com1.Parameters.AddWithValue("@acctNo", tempAcctNo);
                com1.Parameters.AddWithValue("@amnt", tempBal);

                mand1.Parameters.AddWithValue("@desc", tIn);
                mand1.Parameters.AddWithValue("@amnt", amount);
                mand1.Parameters.AddWithValue("@acctNo", tempAcctNo);
                mand1.Parameters.AddWithValue("@created", DateTime.Now);
                mand1.Parameters.AddWithValue("@id", inID);

                try
                {
                    con.Open();

                    int done = com.ExecuteNonQuery();
                    int made = mand.ExecuteNonQuery();

                    int done1 = com1.ExecuteNonQuery();
                    int made1 = mand1.ExecuteNonQuery();


                    if (done >= 0 && made >= 0 && done1 >= 0 && made1 >= 0)
                    {
                        message = "Transfer Successful";
                    }
                    con.Close();
                }
                catch (Exception ex)
                {
                    con.Close();
                    message = "Error: " + ex.Message;
                }               
            }
            else message = "Insufficient Balance";

            return message;
        }
    }
}
