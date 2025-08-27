using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BossTime
{
    public class APIHandler
    {
        string APIKey = "";
        string JsonFilePath;
        string dbID = "YOURUSERNAME";
        string dbPass = "YOURPASSWORD";
        string server = "YOURSERVER\\SQLEXPRESS";
        ServerData serverData = null;

        public APIHandler(string fpath)
        {
            JsonFilePath = fpath;


      
        }

        // Gets the current boss minute from the database or cache
        public int GetBossMinute(bool force=false)
        {
            FileInfo fi = new FileInfo(JsonFilePath);
            if(!force && serverData != null && (DateTime.UtcNow - serverData.RequestTime.ToUniversalTime()).TotalMinutes < 5)
            {
                return serverData.BossMinute;
            } else if (!force && fi.Exists && (DateTime.Now - fi.LastWriteTime).TotalMinutes < 5)
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
                ds.ConnectionString = $"Data Source={server};Initial Catalog=ServerDB;User ID={dbID};Password={dbPass}";
                ds.SelectCommand = "SELECT TOP 1 Value FROM Metadata WHERE [Key] = 'boss.time.second'";
                DataView dv = (ds.Select(DataSourceSelectArguments.Empty) as DataView);
                DataTable dt = dv.ToTable();
                int minute = (int)dt.Rows[0][0];
                Debug.WriteLine(minute);

                return minute;
            }

            return 0;

        }



        // Gets the list of maps with bosses and their spawn times
        private List<Map> MapList()
        {
            List<Map> maps = new List<Map>();
            SqlDataSource ds = new SqlDataSource();
            ds.ConnectionString = $"Data Source={server};Initial Catalog=GameDB;User ID={dbID};Password={dbPass}";
            ds.SelectCommand = "SELECT MapList.Name, MapList.LevelReq, MapMonster.Stage, MapMonster.BossMonster1,MapMonster.HoursBossMonster1 FROM MapMonster INNER JOIN MapList ON MapMonster.Stage = MapList.ID WHERE BossMonster1 IS NOT NULL AND Name IS NOT NULL AND HoursBossMonster1 IS NOT NULL;";
            DataView dv = ds.Select(DataSourceSelectArguments.Empty) as DataView;
            DataTable dt = dv.ToTable();
            foreach(DataRow dr in dt.Rows)
            {
                try { 
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

            return maps;
        }

        // Gets the full API data, including boss minute and map list, from cache or database
        public ServerData ApiData(bool force=false)
        {
            FileInfo fi = new FileInfo(JsonFilePath);
            if(!force && serverData != null && (DateTime.UtcNow - serverData.RequestTime.ToUniversalTime()).TotalMinutes < 5)
            {
                return serverData;
            } else if (!force && fi.Exists && (DateTime.Now - fi.LastWriteTime).TotalMinutes < 5)
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
                sd.RequestTime = DateTime.Now;
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

    }

    // Data structure for the API response
    public class ServerData
    {
        public int BossMinute { get; set; } = 0;
        public List<Map> Maps { get; set; } = new List<Map>();
        public DateTime RequestTime { get; set; } = DateTime.MinValue;
    }

    public class Map
    {
        public int MapID { get; set; }

        public bool Arena { get; set; } = false;
        public string MapName { get; set; }
        public string BossName { get; set; }
        public int MapLevel { get; set; } = 0;
        public List<int> Spawns { get; set; }
    }


}


    


