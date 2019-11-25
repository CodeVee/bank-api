using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AdminPoint
{
    public class AdminFunctions
    {
        public static SqlConnection con = new SqlConnection("Data Source=DESKTOP-4F2UI6D;Initial Catalog=BankApp;Integrated Security=True");
        public static string salt = "EDITH";
       
        public static string AdminLogin(AdminInfo info)
        {

            string username = info.Username;
            string password = info.Password;
            string encrypt = SHA1(password + salt);
            string login = " Invalid Login Details";

            string query = "SELECT * FROM Administrators WHERE AdminUsername = @username AND AdminPassword = @hash";
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

        public static string CreateAccount(CreateUser user)
        {
            string fName = user.FirstName;
            string lName = user.LastName;
            string email = user.Email;
            string phone = user.Phone;
            string username = user.Username;
            string password = user.Password;
            string acctType = user.AcctType;
            string acctNo = AcctNoGen(acctType);
            double acctBal = user.AcctBal;
            int AdminID = ID(user.Admin);

            string message = "";
            

            bool uniqueUser = Unique(username);
            bool uniqueEmail = UniqueEmail(email);
            bool uniquePhone = UniquePhone(phone);

            if (uniqueUser) message = "Username Already exist";
            else if (uniqueEmail) message = "Email Already exist";
            else if (uniquePhone) message = "Phone Already exist";
            else if (AdminID < 1) message = "Enter Valid Username";
            else
            {
                
                string hashedPassword = SHA1(password + salt);
                string query = "insert into Customers values(@fName, @lName, @email, @phone, @username, @hashed, @adminID, @created)";

                SqlCommand command = new SqlCommand(query, con);

                command.Parameters.AddWithValue("@fName", fName);
                command.Parameters.AddWithValue("@lName", lName);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@phone", phone);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@hashed", hashedPassword);
                command.Parameters.AddWithValue("@adminID", AdminID);
                command.Parameters.AddWithValue("@created", DateTime.Now);

                try
                {
                    con.Open();
                    int check = command.ExecuteNonQuery();
                    if (check >= 0)
                    {
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    message = "The following error occurred during the write operation:" + ex.Message;
                    con.Close();
                }


                string secQuery = "SELECT CustomerID FROM Customers WHERE CustomerUsername =@username";
                SqlCommand sql = new SqlCommand(secQuery, con);
                sql.Parameters.AddWithValue("@username", username);
                int id = 0;

                try
                {
                    con.Open();
                    SqlDataReader read = sql.ExecuteReader();

                    if (read.Read())
                    {
                        id = read.GetInt32(0);
                        con.Close();
                    }
                }
                catch (Exception mess)
                {
                    message = "Error " + mess.Message;
                    con.Close();
                }


                string thirdQuery = "insert into Accounts values(@acctNo, @acctType, @acctBal, @id, @aID, @created)";
                SqlCommand sqlCommand = new SqlCommand(thirdQuery, con);

                sqlCommand.Parameters.AddWithValue("@acctNo", acctNo);
                sqlCommand.Parameters.AddWithValue("@acctType", acctType);
                sqlCommand.Parameters.AddWithValue("@id", id);
                sqlCommand.Parameters.AddWithValue("@aID", AdminID);
                sqlCommand.Parameters.AddWithValue("@created", DateTime.Now);

                string fourthQuery = "UPDATE LatestNumbers SET AccountNumber = @acctNo WHERE AccountType = @acctType";
                SqlCommand update = new SqlCommand(fourthQuery, con);

                update.Parameters.AddWithValue("@acctNo", acctNo);
                update.Parameters.AddWithValue("@acctType", acctType);

                try
                {
                    double bal = acctBal;
                    sqlCommand.Parameters.AddWithValue("@acctBal", bal);

                    con.Open();
                    int done = sqlCommand.ExecuteNonQuery();
                    int yes = update.ExecuteNonQuery();

                    if (done >= 0 && yes >= 0)
                    {
                        message = "Account Created Successfully";
                        con.Close();
                    }

                }
                catch (Exception final)
                {
                    message = "Error3: " + final.Message;
                    con.Close();
                }
            }
            return message;
        }

        public static string AcctNoGen(string type)
        {
            string acctNo = "Unknown";
            int accNo = 0;

            string queryAcc = "SELECT AccountNumber from LatestNumbers WHERE AccountType=@type";

            SqlCommand comd = new SqlCommand(queryAcc, con);
            comd.Parameters.AddWithValue("@type", type);

            try
            {
                con.Open();
                SqlDataReader accDits = comd.ExecuteReader();

                if (accDits.Read())
                {
                    accNo = int.Parse(accDits.GetString(0));
                    Random account = new Random();
                    int accNumber = account.Next(accNo + 1, accNo + 3);
                    acctNo = accNumber.ToString();
                    con.Close();
                }
            }
            catch (Exception ex)
            {

                acctNo = "Error: " + ex.Message;
                con.Close();
            }
            return acctNo;
        }

        public static bool Unique(string username)
        {
            string query = "SELECT * FROM Customers WHERE CustomerUsername=@user";
            bool uniq = false;
            SqlCommand command = new SqlCommand(query, con);
            command.Parameters.AddWithValue("@user", username);

            try
            {
                con.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    con.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {

                query = "Error " + ex.Message;
                con.Close();
            }
            con.Close();
            return uniq;
        }

        public static bool UniqueEmail(string email)
        {
            string query = "SELECT * FROM Customers WHERE CustomerEmail= @email";
            bool uniq = false;
            SqlCommand command = new SqlCommand(query, con);
            command.Parameters.AddWithValue("@email", email);

            try
            {
                con.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    con.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {

                query = "Error " + ex.Message;
                con.Close();
            }
            con.Close();
            return uniq;
        }

        public static bool UniquePhone(string phone)
        {
            string query = "SELECT * FROM Customers WHERE CustomerPhone= @phone";
            bool uniq = false;
            SqlCommand command = new SqlCommand(query, con);
            command.Parameters.AddWithValue("@phone", phone);

            try
            {
                con.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    con.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {

                query = "Error " + ex.Message;
                con.Close();
            }
            con.Close();
            return uniq;
        }

        public static int ID(string user)
        {
            
            int id = 0;

            string query = "SELECT AdminID FROM Administrators WHERE AdminUsername = @user";
            SqlCommand command = new SqlCommand(query, con);

            command.Parameters.AddWithValue("@user", user);
            try
            {
                con.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = reader.GetInt32(0);
                }
                
                con.Close();
            }
            catch (Exception)
            {
                con.Close();
            }
            return id;
        }
    }
}
