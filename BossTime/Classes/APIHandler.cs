using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace BossTime
{
    public class APIHandler
    {

        string JsonFilePath;

        ServerData serverData = null;

        public APIHandler(string fpath="")
        {
            JsonFilePath = fpath;



        }

    #region BOSS_TIME

        // Gets the current boss minute from the database or cache
        public int GetBossMinute(bool force = false)
        {
            try
            {   
                FileInfo fi = new FileInfo(JsonFilePath);
                if (!force && serverData != null && (DateTime.UtcNow - serverData.RequestTime.ToUniversalTime()).TotalMinutes < 5)
                {
                    return serverData.BossMinute;
                }
                else if (!force && fi.Exists && (DateTime.Now - fi.LastWriteTime).TotalMinutes < 5)
                {
                    try
                    {
                        string json = File.ReadAllText(JsonFilePath);
                        serverData = JsonConvert.DeserializeObject<ServerData>(json);
                        if (serverData != null)
                        {
                            return serverData.BossMinute;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                }
                else
                {
                    SqlDataSource ds = new SqlDataSource();
                    ds.ConnectionString = $"Data Source={DBCredentials.server};Initial Catalog=ServerDB;User ID={DBCredentials.dbID};Password={DBCredentials.dbPass}";
                    ds.SelectCommand = "SELECT TOP 1 Value FROM Metadata WHERE [Key] = 'boss.time.second'";
                    DataView dv = (ds.Select(DataSourceSelectArguments.Empty) as DataView);
                    DataTable dt = dv.ToTable();
                    int minute = (int)dt.Rows[0][0];
                    Debug.WriteLine(minute);

                    return minute;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return 0;
            }

        }



        // Gets the list of maps with bosses and their spawn times
        private List<Map> MapList()
        {
            List<Map> maps = new List<Map>();
            try
            {
                SqlDataSource ds = new SqlDataSource();
                ds.ConnectionString = $"Data Source={DBCredentials.server} ;Initial Catalog=GameDB;User ID= {DBCredentials.dbID} ;Password= {DBCredentials.dbPass}";
                ds.SelectCommand = "SELECT MapList.Name, MapList.LevelReq, MapMonster.Stage, MapMonster.BossMonster1,MapMonster.HoursBossMonster1 FROM MapMonster INNER JOIN MapList ON MapMonster.Stage = MapList.ID WHERE BossMonster1 IS NOT NULL AND Name IS NOT NULL AND HoursBossMonster1 IS NOT NULL;";
                DataView dv = ds.Select(DataSourceSelectArguments.Empty) as DataView;
                DataTable dt = dv.ToTable();
                foreach (DataRow dr in dt.Rows)
                {
                    try
                    {
                        Map m = new Map();

                        m.MapID = int.Parse(dr["Stage"].ToString());
                        m.MapName = (string)dr["Name"];
                        m.BossName = (string)dr["BossMonster1"];
                        m.MapLevel = (int)dr["LevelReq"];
                        string hours = (string)dr["HoursBossMonster1"];
                        List<int> spawns = new List<int>();
                        foreach (string s in hours.Split(' '))
                        {
                            if (int.TryParse(s, out int val))
                            {
                                spawns.Add(val);
                            }
                        }
                        m.Spawns = spawns;
                        maps.Add(m);
                        Debug.WriteLine($"{m.MapID} {m.MapName} {m.BossName} {string.Join(",", m.Spawns)}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return maps;
        }

        // Gets the full API data, including boss minute and map list, from cache or database
        public ServerData ApiData(bool force = false)
        {
            FileInfo fi = new FileInfo(JsonFilePath);
            if (!force && serverData != null && (DateTime.UtcNow - serverData.RequestTime.ToUniversalTime()).TotalMinutes < 5)
            {
                return serverData;
            }
            else if (!force && fi.Exists && (DateTime.Now - fi.LastWriteTime).TotalMinutes < 5)
            {
                try
                {
                    string json = File.ReadAllText(JsonFilePath);
                    ServerData sd = JsonConvert.DeserializeObject<ServerData>(json);
                    if (sd != null)
                    {
                        return sd;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            else
            {

                ServerData sd = new ServerData();
                sd.BossMinute = GetBossMinute();
                sd.Maps = MapList();
                sd.RequestTime = DateTime.UtcNow;
                string jstxt = JsonConvert.SerializeObject(sd, Formatting.Indented);
                try
                {
                    File.WriteAllText(JsonFilePath, jstxt);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
                return sd;
            }

            return new ServerData();
        }

    #endregion

    #region ACCOUNT_REGISTRATION
        // Registers a new account with the given username, password, and email
        // Returns an APIResponse indicating success or failure
        public APIResponse RegisterAccount(string username, string password, string email)
        {


            APIResponse response = new APIResponse() { Success = false, Message = "Default" };
            // Basic validation
            // 1. Check for allowed characters
            // 2. Check for length
            // 3. Check for empty fields
            // 4. Check for spaces
            // 5. Check if username or email already exists
            // 6. Hash password
            // 7. Insert into database
            // 8. Return success or failure message
            
            foreach (char c in username)
            {
                // Check if character is in allowed list
                if (RegisterVariables.AllowedUsernameChars.IndexOf(c) < 0)
                {
                    response.Success = false;
                    response.Message = "Username can only contain letters and numbers";
                    return response;
                }
            }

            foreach(char c in password)
            {
                // Check if character is in allowed list
                if (RegisterVariables.AllowedPasswordChars.IndexOf(c) < 0)
                {
                    response.Success = false;
                    response.Message = "Password contains invalid characters";
                    return response;
                }
            }

            // Check for empty fields
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                response.Success = false;
                response.Message = "Username, password, and email cannot be empty.";
                return response;
            }

            // Check for spaces
            if (username.Contains(" ") || email.Contains(" ") || password.Contains(" "))
            {
                response.Success = false;
                response.Message = "Username, Password and Email cannot contain spaces";
                return response;
            }

            // Check for username length
            if (username.Length < RegisterVariables.MinUsernameLength || username.Length > RegisterVariables.MaxUsernameLength)
            {
                response.Success = false;
                response.Message = $"Username must be between {RegisterVariables.MinUsernameLength} and {RegisterVariables.MaxUsernameLength} characters.";
                return response;
            }

            // Check password length
            if (password.Length < RegisterVariables.MinPasswordLength || password.Length > RegisterVariables.MaxPasswordLength)
            {
                response.Success = false;
                response.Message = $"Password must be between {RegisterVariables.MinPasswordLength} and {RegisterVariables.MaxPasswordLength} characters.";
                return response;
            }

            // Check email length and format
            if (!email.Contains("@") || !email.Contains(".") || email.Length < RegisterVariables.MinEmailLength || email.Length > RegisterVariables.MaxEmailLength)
            {
                response.Success = false;
                response.Message = $"Email must be a valid email address between {RegisterVariables.MinEmailLength} and {RegisterVariables.MaxEmailLength} characters.";
                return response;
            }



            try
            {
               
                SqlDataSource ds = new SqlDataSource();
                ds.SelectParameters.Clear();
                ds.SelectParameters.Add("acc", username);
                ds.SelectParameters.Add("email", email);
                ds.ConnectionString = $"Data Source={DBCredentials.server}  ;Initial Catalog=UserDB;User ID=  {DBCredentials.dbID}  ;Password=  {DBCredentials.dbPass}";
                ds.SelectCommand = "SELECT AccountName from UserInfo WHERE AccountName=@acc OR Email=@email";
                DataView dv = ds.Select(DataSourceSelectArguments.Empty) as DataView;
                DataTable dt = dv.ToTable();

                if (dt.Rows.Count > 0)
                {
                    // Username or email already exists
                    response.Success = false;
                    response.Message = "Account name or email already exists.";
                    return response;
                }
                else
                {
                    
                    string finalpass = HashPass(username,password);
                    ds.InsertParameters.Clear();

                    // Add parameters for insertion
                    ds.InsertParameters.Add("acc", username);
                    ds.InsertParameters.Add("email", email);
                    ds.InsertParameters.Add("pass", finalpass);
                    ds.InsertParameters.Add("dbdate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                    ds.InsertParameters.Add("coins", RegisterVariables.NewAccountStartingCoins.ToString());
                    int PreActivation = 98;
                    if(SystemVariables.RequireEmailAccountActivation)
                    {
                        PreActivation = 0;
                    }
                    ds.InsertParameters.Add("preauth", PreActivation.ToString());




                    ds.InsertCommand = "INSERT INTO UserInfo (AccountName, Password, Email, RegisDay, Flag,Coins,GameMasterType,GameMasterLevel,GameMasterMacAddress,CoinsTraded,BanStatus,IsMuted,MuteCount,ActiveCode,Active) VALUES (@acc, @pass, @email, @dbdate,@preauth,@coins,0,0,'',0,0,0,0,1,0);";
                    ds.Inserted += new SqlDataSourceStatusEventHandler((snd, e) => {

                       foreach(DbParameter pr in e.Command.Parameters)
                        {
                            Debug.WriteLine(pr.ParameterName + " - " + pr.Value);
                        }

                        if (e.Exception != null)
                        {
                            throw e.Exception;
                        }
                        else
                        {
                            response.Success = true;
                            response.Message = $"Account Created for {username}";
                            
                        }

                      

                    });

                    ds.Insert();
                    return response;




                }



                return response;

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred during registration. " + ex.Message.ToString();
                return response;
            }

        }


        public APIResponse ActivateAccount(string username, string email)
        {
            APIResponse response = new APIResponse() { Success = false, Message = "Default" };
            try
            {
                SqlDataSource ds = new SqlDataSource();
                ds.SelectParameters.Clear();
                ds.SelectParameters.Add("acc", username);
                ds.SelectParameters.Add("email", email);
                ds.ConnectionString = $"Data Source={DBCredentials.server}  ;Initial Catalog=UserDB;User ID=  {DBCredentials.dbID}  ;Password=  {DBCredentials.dbPass}";
                ds.SelectCommand = "SELECT AccountName from UserInfo WHERE AccountName=@acc AND Email=@email AND Flag=0";
                DataView dv = ds.Select(DataSourceSelectArguments.Empty) as DataView;
                DataTable dt = dv.ToTable();
                if (dt.Rows.Count == 0)
                {
                    Debug.WriteLine("ROWS 0");
                    
                    response.Success = false;
                    response.Message = "Account not found or already activated.";
                    return response;
                }
                else
                {
                    Debug.WriteLine("ROWS NOT 0");
                    ds.UpdateParameters.Clear();
                    // Add parameters for update
                    ds.UpdateParameters.Add("active", "98");
                    ds.UpdateParameters.Add("acc", username);
                    ds.UpdateCommand = "UPDATE UserInfo SET Flag=@active WHERE AccountName=@acc;";
                    ds.Updated += new SqlDataSourceStatusEventHandler((snd, e) =>
                    {
                        foreach (DbParameter pr in e.Command.Parameters)
                        {
                            Debug.WriteLine(pr.ParameterName + " - " + pr.Value);
                        }
                        if (e.Exception != null)
                        {
                            Debug.WriteLine("EXCEPTION UPDATING");
                            throw e.Exception;
                        }
                        else
                        {
                            Debug.WriteLine("SUCCESS");
                            response.Success = true;
                            response.Message = $"Account Activated for {username}";
                        }
                    });
                    ds.Update();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FAILED");
                response.Success = false;
                response.Message = "An error occurred during account activation. " + ex.Message.ToString();
                return response;
            }

            return response;
        }


        #endregion

            #region ACCOUNT_LOGIN
            // Logs in an account with the given username and password
            /// <summary>
            /// Logs in an account with the given username and password.
            /// Returns an APIResponse indicating success or failure.
            /// If successful, the Data field contains encrypted login data.
            /// The login data includes the username, user ID, login date, and a unique GUID.
            /// </summary>
            /// <param name="username"></param>
            /// <param name="password"></param>
            /// <returns></returns>
        public APIResponse LoginAccount(string username, string password)
        {


            APIResponse response = new APIResponse() { Success = false, Message = "Default" };
 

            try
            {

                SqlDataSource ds = new SqlDataSource();
                ds.SelectParameters.Clear();
                ds.SelectParameters.Add("acc", username.Trim());
                string hp = HashPass(username, password).Trim();
                Debug.WriteLine(hp);
                ds.SelectParameters.Add("pass", hp);
                ds.ConnectionString = $"Data Source={DBCredentials.server};Initial Catalog=UserDB;User ID={DBCredentials.dbID};Password={DBCredentials.dbPass}";
                ds.SelectCommand = "SELECT AccountName,ID,BanStatus,Flag from UserInfo WHERE AccountName=@acc AND Password=@pass";
                DataView dv = ds.Select(DataSourceSelectArguments.Empty) as DataView;
                DataTable dt = dv.ToTable();
                Debug.WriteLine(dt.Rows.Count);
                if (dt.Rows.Count > 0)
                {

                    if (dt.Rows[0]["BanStatus"] != null && int.TryParse(dt.Rows[0]["BanStatus"].ToString(), out int banstatus))
                    {
                        if (banstatus != 0)
                        {
                            response.Success = false;
                            response.Message = "Account is banned.";
                            return response;
                        }
                    }

                    if (dt.Rows[0]["Flag"] != null && int.TryParse(dt.Rows[0]["Flag"].ToString(), out int flag))
                    {
                        if (flag < 98)
                        {
                            response.Success = false;
                            response.Message = "Account is not activated.";
                            return response;
                        }
                    }

                    // User, data correct exists
                    response.Success = true;
                    response.Message = "Login Successful!";

                    LoginData ld = new LoginData()
                    {
                        Username = dt.Rows[0]["AccountName"].ToString(),
                        ID = dt.Rows[0]["ID"].ToString(),
                        LoginDate = DateTime.UtcNow,
                        GUID = Guid.NewGuid().ToString()
                    };
                    string ldjson = JsonConvert.SerializeObject(ld);

                    string encdata = Encrypt(ldjson);

                    response.Data = encdata;

                    return response;
                }
                else
                {
                    // Invalid credentials
                    response.Success = false;
                    response.Message = "Login Failed!";
                    return response;
                }

            }
            catch (Exception ex)
            {
                response.Success = false;
                Debug.WriteLine(ex);
                response.Message = "An error occurred during login. " + ex.Message.ToString();
                return response;
            }

        }

        #region ACCOUNT_TOKEN_VERIFICATION
        // Validates the given token and returns the associated account data if valid
        /// <summary>
        /// Validates the given token and returns the associated account data if valid.
        /// The token is decrypted to retrieve the login data.
        /// The login data is checked for validity, including username, user ID, GUID, and login date (must be within 1 day).
        /// If valid, the method returns success with the account data.
        /// If invalid or expired, the method returns failure with an appropriate message.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public APIResponse ValidateToken(string token)
        {
            APIResponse response = new APIResponse() { Success = false, Message = "Default" };
            try
            {
                string decdata = Decrypt(token);
                LoginData ld = JsonConvert.DeserializeObject<LoginData>(decdata);
                if (ld != null && !string.IsNullOrWhiteSpace(ld.Username) && !string.IsNullOrWhiteSpace(ld.ID) && !string.IsNullOrWhiteSpace(ld.GUID) && (DateTime.UtcNow - ld.LoginDate.ToUniversalTime()).TotalDays <= 1)
                {
                    response.Success = true;
                    response.Message = "Token is valid.";
                    response.Data = new AccountData(ld);
                    return response;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Token is invalid or expired.";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                Debug.WriteLine(ex.ToString());
                response.Message = "An error occurred during token validation. " + ex.ToString();
                return response;
            }
        }
        #endregion

        public int GetCoins(LoginData ld)
        {
            try
            {
                SqlDataSource ds = new SqlDataSource();
                ds.SelectParameters.Clear();
                ds.SelectParameters.Add("id", ld.ID.Trim());

                ds.ConnectionString = $"Data Source={DBCredentials.server};Initial Catalog=UserDB;User ID={DBCredentials.dbID};Password={DBCredentials.dbPass}";
                ds.SelectCommand = "SELECT Coins from UserInfo WHERE ID=@id";
                DataView dv = ds.Select(DataSourceSelectArguments.Empty) as DataView;
                DataTable dt = dv.ToTable();
                Debug.WriteLine(dt.Rows.Count);
                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["Coins"] != null && int.TryParse(dt.Rows[0]["Coins"].ToString(), out int coins))
                    {
                        return coins;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    // Invalid credentials
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return 0;
            }
        }

        #endregion

    #region STRIPE_PAYMENT_HANDLING


        // Registers a new payment intent in the database
        /// <summary>
        /// Registers a new payment intent in the database.
        /// The payment intent includes details such as username, user hash, package ID, package name, amount, coins, payment link ID, creation date, completion status, unique key, payment event ID, and request ID.
        /// If the registration is successful, it returns a success message.
        /// If an error occurs, it returns an appropriate error message.
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        public APIResponse RegisterPaymentIntent(ptPaymentIntent intent)
        {
            APIResponse response = new APIResponse() { Success = false, Message = "Default" };
            try
            {
                SqlDataSource ds = new SqlDataSource();
                ds.ConnectionString = $"Data Source={DBCredentials.server};Initial Catalog=UserDB;User ID={DBCredentials.dbID};Password={DBCredentials.dbPass}";
                ds.InsertParameters.Clear();
                // Add parameters for insertion
                ds.InsertParameters.Add("uname", intent.Username);
                ds.InsertParameters.Add("uid", intent.UserHash);
                ds.InsertParameters.Add("pid", intent.PackageID);
                ds.InsertParameters.Add("pname", intent.PackageName);
                ds.InsertParameters.Add("amt", intent.Amount.ToString());
                ds.InsertParameters.Add("coins", intent.Coins.ToString());
                ds.InsertParameters.Add("paymentlinkid", intent.PaymentLinkID);
                ds.InsertParameters.Add("createdat", intent.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                ds.InsertParameters.Add("completed", intent.Completed ? "1" : "0");
                ds.InsertParameters.Add("ukey", intent.PaymentUniqueKey);
                ds.InsertParameters.Add("evid", intent.PaymentEventID);
                ds.InsertParameters.Add("rqid", intent.RequestID);
                

                ds.InsertCommand = "INSERT INTO StripeEvents (Username,UserHash,PackageID,PackageName,PaymentAmount,CoinAmount,StripeEventId,StripePaymentLinkId,Created,Completed,UniqueKey,ReqID) VALUES (@uname,@uid,@pid,@pname,@amt,@coins,@evid,@paymentlinkid,@createdat,@completed,@ukey,@rqid);";
                ds.Insert();
                response.Success = true;
                response.Message = "Payment intent registered successfully.";
                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                response.Success = false;
                response.Message = "An error occurred during payment intent registration.";
                return response;
            }
        }

        // Updates the payment intent as completed and updates user coins
        /// <summary>
        /// Updates the payment intent as completed and updates user coins.
        /// First, it checks if the payment intent matches the user using the user hash, username, payment link ID, and unique key.
        /// If the check passes, it updates the payment intent in the database to mark it as completed.
        /// Then, it updates the user's coin balance by adding the purchased coins.
        /// If any step fails, it returns an appropriate error message.
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        public APIResponse UpdatePaymentCompleted(ptPaymentIntent intent)
        {
            APIResponse response = new APIResponse() { Success = false, Message = "Default" };



            try
            {

                string PaymentEmail = CheckPaymentAgainstUser(intent.UserHash, intent.Username, intent.PaymentLinkID, intent.PaymentUniqueKey);

                if (string.IsNullOrWhiteSpace(PaymentEmail))
                {
                    response.Success = false;
                    response.Message = "Payment intent does not match user.";
                    return response;
                }
                else
                {
                    SqlDataSource ds = new SqlDataSource();
                    ds.ConnectionString = $"Data Source={DBCredentials.server};Initial Catalog=UserDB;User ID={DBCredentials.dbID};Password={DBCredentials.dbPass}";
                    ds.UpdateParameters.Clear();
                    // Add parameters for update
                    ds.UpdateParameters.Add("completed", "1");
                    ds.UpdateParameters.Add("plink", intent.PaymentLinkID);
                    ds.UpdateParameters.Add("ukey", intent.PaymentUniqueKey);
                    ds.UpdateCommand = "UPDATE StripeEvents SET Completed=@completed WHERE StripePaymentLinkId=@plink and UniqueKey=@ukey;";
                    ds.Updated += new SqlDataSourceStatusEventHandler((snd, e) =>
                    {
                        foreach (DbParameter pr in e.Command.Parameters)
                        {
                            Debug.WriteLine(pr.ParameterName + " - " + pr.Value);
                        }
                        if (e.Exception != null)
                        {
                            throw e.Exception;
                        }
                        else
                        {
                            response.Success = true;
                            response.Message = $"Payment intent updated for {intent.Username}";
                        }

                    });
                    ds.Update();
                    bool CoinsUpdate = UpdateUserCoins(intent.Username, intent.Coins);

                    if (!CoinsUpdate)
                    {
                        response.Success = false;
                        response.Message = "Payment updated, but failed to update user coins.";
                    }
                    else
                    {
                        response.Success = true;
                        response.Message = "Payment and coins updated successfully.";
                        string html = $"<h1>Thank you for your purchase from {SystemVariables.ServerName}!</h1><br /><p>Your purchase of {intent.Coins} coins for {intent.PackageName} has been successfully processed.</p><br /><p>If you did not make this purchase, please contact support immediately.</p><br /><p>Best regards,<br />The {SystemVariables.ServerName} Team</p>";
                        EmailHandler.SendEmail(PaymentEmail, $"Thank you for your purchase from {SystemVariables.ServerName}!", html, SystemVariables.ServerName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                response.Success = false;
                response.Message = "An error occurred during payment intent update.";
                return response;
            }


            return response;
        }

        // Check if the payment intent matches the user
        /// <summary>
        /// Check if the payment intent matches the user.
        /// First, it checks if the user hash matches the username and user ID.
        /// If the user hash is valid, it queries the database to check if there is a matching payment intent with the given user hash, username, payment link ID, and unique key.
        /// If a match is found, it returns true, otherwise false.
        /// If the user hash is invalid, it returns false.
        /// </summary>
        /// <param name="userhash"></param>
        /// <param name="usern"></param>
        /// <param name="paymentlink"></param>
        /// <param name="uniquekey"></param>
        /// <returns></returns>
        private string CheckPaymentAgainstUser(string userhash, string usern, string paymentlink, string uniquekey)
        {

            try
            {
                bool UserValidHash = CheckUserHash(usern, userhash);

                if (UserValidHash)
                {

                    SqlDataSource ds = new SqlDataSource();
                    ds.SelectParameters.Clear();
                    ds.SelectParameters.Add("uid", userhash.Trim());
                    ds.SelectParameters.Add("uname", usern.Trim());
                    ds.SelectParameters.Add("plink", paymentlink.Trim());
                    ds.SelectParameters.Add("ukey", uniquekey.Trim());
                    ds.ConnectionString = $"Data Source={DBCredentials.server};Initial Catalog=UserDB;User ID={DBCredentials.dbID};Password={DBCredentials.dbPass}";
                    ds.SelectCommand = "SELECT Username, Email from StripeEvents WHERE UserHash=@uid AND Username=@uname AND StripePaymentLinkId=@plink AND UniqueKey=@ukey";
                    DataView dv = ds.Select(DataSourceSelectArguments.Empty) as DataView;
                    DataTable dt = dv.ToTable();
                    Debug.WriteLine(dt.Rows.Count);
                    if (dt.Rows.Count > 0)
                    {
                        string email = dt.Rows[0]["Email"].ToString();
                        return email;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return string.Empty;
        }


        // Check if the user hash matches the username and user ID
        /// <summary>
        /// Checks if the user hash matches the username and user ID.
        /// The user hash is generated by hashing the username and user ID using SHA256.
        /// If the hash matches, the method returns true, otherwise false.
        /// </summary>
        /// <param name="usern"></param>
        /// <param name="userhash"></param>
        /// <returns></returns>
        private bool CheckUserHash(string usern, string userhash)
        {
            try
            {
                SqlDataSource ds = new SqlDataSource();
                ds.SelectParameters.Clear();
                ds.SelectParameters.Add("acc", usern.Trim());
                ds.ConnectionString = $"Data Source={DBCredentials.server};Initial Catalog=UserDB;User ID={DBCredentials.dbID};Password={DBCredentials.dbPass}";
                ds.SelectCommand = "SELECT AccountName, ID from UserInfo WHERE AccountName=@acc";
                DataView dv = ds.Select(DataSourceSelectArguments.Empty) as DataView;
                DataTable dt = dv.ToTable();
                Debug.WriteLine(dt.Rows.Count);
                if (dt.Rows.Count > 0)
                {
                    string userid = dt.Rows[0]["ID"].ToString();
                    string checkhash = HashPass(usern, userid);

                    if(checkhash == userhash)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return false;
        }



        public bool UpdateUserCoins(string usern, int coins)
        {
            try
            {
                SqlDataSource ds = new SqlDataSource();
                ds.ConnectionString = $"Data Source={DBCredentials.server};Initial Catalog=UserDB;User ID={DBCredentials.dbID};Password={DBCredentials.dbPass}";
                ds.UpdateParameters.Clear();
                // Add parameters for update
                ds.UpdateParameters.Add("coins", coins.ToString());
                ds.UpdateParameters.Add("uname", usern);
                ds.UpdateCommand = "UPDATE UserInfo SET Coins=Coins+@coins WHERE AccountName=@uname;";
                ds.Updated += new SqlDataSourceStatusEventHandler((snd, e) =>
                {
                    foreach (DbParameter pr in e.Command.Parameters)
                    {
                        Debug.WriteLine(pr.ParameterName + " - " + pr.Value);
                    }
                    if (e.Exception != null)
                    {
                        throw e.Exception;
                    }
                    else
                    {
                        Debug.WriteLine($"Coins updated for {usern}");
                    }
                });
                ds.Update();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }



    #endregion

    #region ENCRYPTION

        // Method to encrypt data
        public static string Encrypt(string plaintext)
        {
            // Create a new instance of the AES encryption algorithm
            using (Aes aes = Aes.Create())
            {
                
                aes.Key = Encoding.UTF8.GetBytes(SystemVariables.EncryptionKey.PadRight(32,'0'));
                Debug.WriteLine(aes.Key.Length);
                aes.IV = SystemVariables.AES_IV;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                // Create the streams used for encryption
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create a CryptoStream using the encryptor
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plaintext);
                        }
                    }
                    // Store the encrypted data in the public static byte array
                    byte[] encryptedData = ms.ToArray();
                    return Convert.ToBase64String(encryptedData);
                }
            }
        }
        // Method to decrypt data
        public static string Decrypt(string ciphertext)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(SystemVariables.EncryptionKey.PadRight(32, '0'));
                aes.IV = SystemVariables.AES_IV;
                                       
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                // Create the streams used for decryption
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(ciphertext)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            } 
        }


        //SHA256 hash function for password hashing
        // Uses the format USERNAME:password with the username in uppercase

        public string HashPass(string usn, string pass)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.ASCII.GetBytes($"{usn.ToUpper()}:{pass}"));
                // Convert byte array to a string
                StringBuilder sb = new StringBuilder();
                foreach(byte b in bytes)
                {
                    // Convert each byte to a hexadecimal string
                    sb.Append(Convert.ToString(b,16).PadLeft(2,'0').ToUpper());
                }

                return sb.ToString();
            }
        }
    }
    #endregion

    #region API_CLASSES

    // Data structure for the API response
    public class ServerData
        {
            public int BossMinute { get; set; } = 0;
            public List<Map> Maps { get; set; } = new List<Map>();
            public DateTime RequestTime { get; set; } = DateTime.MinValue;
        }

    // Data structure for a map with boss information
    public class Map
        {
            public int MapID { get; set; }

            public bool Arena { get; set; } = false;
            public string MapName { get; set; }
            public string BossName { get; set; }
            public int MapLevel { get; set; } = 0;
            public List<int> Spawns { get; set; }
        }

    // Data structure for a payment intent
    /// <summary>
    /// Data structure for a payment intent
    /// Includes username, user hash, payment event ID, payment unique key, payment link ID, package name, package ID, amount, coins, creation date, and completion status.
    /// Used for registering and updating payment intents in the database.
    /// </summary>
    public class ptPaymentIntent
    {
        public string Username { get; set; }

        public string UserHash { get; set; }

        public string PaymentEventID { get; set; }

        public string PaymentUniqueKey { get; set; }

        public string PaymentLinkID { get; set; }

        public string PackageName { get; set; }

        public string PackageID { get; set; }
        
        public string RequestID { get; set; }

        public int Amount { get; set; }
        public int Coins { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Completed { get; set; } = false;

        public ptPaymentIntent(string username, string userhash, string packageid, string packagename, int amount, int coins, string paymentlinkid, string paymentEventID, string paymentUniqueKey, string requestID)
        {
            Username = username;
            PaymentLinkID = paymentlinkid;
            UserHash = userhash;
            PackageID = packageid;
            PackageName = packagename;
            Amount = amount;
            Coins = coins;
            PaymentEventID = paymentEventID;
            PaymentUniqueKey = paymentUniqueKey;
            RequestID = requestID;
        }

    }


    public class LoginData
    {
        public string Username { get; set; }


        public string ID { get; set; }
        public DateTime LoginDate { get; set; }

        public string GUID { get; set; }
    }

    public class AccountData
    {
        public string Username { get; set; }

        public string ID { get; set; }

        public int Coins { get; set; }

        public DateTime LoginDate { get; set; }

        public string GUID { get; set; }

        public string AccountHash { get; set; }


        public AccountData(LoginData ld)
        {
            Username = ld.Username;
            ID = ld.ID;
            LoginDate = ld.LoginDate;
            GUID = ld.GUID;
            APIHandler api = new APIHandler();
            Coins = api.GetCoins(ld);
            AccountHash = api.HashPass(ld.Username, ld.ID);
        }



    }

    public class EmailAuthentication
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public string Randomizer { get; set; } = Guid.NewGuid().ToString();

        public EmailAuthentication(string email, string username)
        {
            Email = email;
            Username = username;
        }

        public override string ToString()
        {
            string code = JsonConvert.SerializeObject(this);
            return code;
        }

    }





    // Standard API response structure
    public class APIResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "";
        public object Data { get; set; } = null;
    }

    #endregion

}





