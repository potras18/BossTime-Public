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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace BossTime
{
    public class APIHandler
    {
        string APIKey = "";
        string JsonFilePath;

        ServerData serverData = null;

        public APIHandler(string fpath="")
        {
            JsonFilePath = fpath;



        }

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





                    ds.InsertCommand = "INSERT INTO UserInfo (AccountName, Password, Email, RegisDay, Active,Coins,GameMasterType,GameMasterLevel,GameMasterMacAddress,CoinsTraded,BanStatus,IsMuted,MuteCount,ActiveCode) VALUES (@acc, @pass, @email, @dbdate,1,@coins,0,0,'',0,0,0,0,1);";
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
                response.Message = "An error occurred during registration. " + ex.ToString();
                return response;
            }

        }

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
                ds.SelectCommand = "SELECT AccountName,ID,BanStatus from UserInfo WHERE AccountName=@acc AND Password=@pass";
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
                response.Message = "An error occurred during login. " + ex.ToString();
                return response;
            }

        }

        public APIResponse ValidateToken(string token)
        {
            APIResponse response = new APIResponse() { Success = false, Message = "Default" };
            try
            {
                string decdata = Decrypt(token);
                LoginData ld = JsonConvert.DeserializeObject<LoginData>(decdata);
                if (ld != null && !string.IsNullOrWhiteSpace(ld.Username) && !string.IsNullOrWhiteSpace(ld.ID) && !string.IsNullOrWhiteSpace(ld.GUID) && (DateTime.UtcNow - ld.LoginDate.ToUniversalTime()).TotalDays <=1)
                {
                    response.Success = true;
                    response.Message = "Token is valid.";
                    response.Data = ld.Username;
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

        private string HashPass(string usn, string pass)
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
                    sb.Append(Convert.ToString(b,16).ToUpper());
                }

                return sb.ToString();
            }
        }
    }
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


    public class LoginData
    {
        public string Username { get; set; }
        public string ID { get; set; }
        public DateTime LoginDate { get; set; }

        public string GUID { get; set; }
    }





    // Standard API response structure
    public class APIResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "";
        public object Data { get; set; } = null;
    }

}


    


